using System.Text.Json;
using LeadGen.Core.Configuration;
using LeadGen.Core.Domain;
using LeadGen.Core.Providers;
using LeadGen.Core.Services;
using LeadGen.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeadGen.Infrastructure.Services;

internal sealed record LeadCandidate(
    SearchResultDto Result,
    string Domain,
    string HomePageUrl,
    int Score,
    bool IsOwnedWebsite = true,
    string? CandidateKey = null);

public sealed class LeadDiscoveryWorkflow : ILeadDiscoveryWorkflow
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly LeadGenDbContext _db;
    private readonly IAiClient _aiClient;
    private readonly IWebSearchClient _searchClient;
    private readonly IContactEnrichmentClient _contactClient;
    private readonly LeadGenOptions _options;
    private readonly ILogger<LeadDiscoveryWorkflow> _logger;

    private static readonly string[] MiddlemanDomains =
    [
        "linkedin.com", "facebook.com", "instagram.com", "youtube.com", "x.com", "twitter.com",
        "medium.com", "substack.com", "wordpress.com", "blogspot.com", "blogger.com",
        "yelp.", "tripadvisor.", "clutch.co", "goodfirms.co", "designrush.com", "sortlist.",
        "upcity.com", "g2.com", "capterra.com", "softwareadvice.com"
    ];

    private static readonly string[] PublicProfileDomains =
    [
        "weddingwire.", "theknot.", "eventective.", "partyslate.", "venuereport.",
        "eventplanner.", "eventseeker.", "foursquare.", "yellowpages.", "europages.",
        "visit", "tourism", "chamber", "hgk.hr"
    ];

    private static readonly string[] InstitutionalNonLeadDomains =
    [
        "trade.gov", "europa.eu", "ec.europa.eu", "gov.uk", ".gov", ".gov.", "state.gov",
        "wikipedia.org", "wikidata.org", "worldbank.org", "oecd.org"
    ];

    private static readonly string[] PublisherOrDirectoryPathTerms =
    [
        "/blog", "/news", "/article", "/articles", "/post", "/posts", "/magazine",
        "/category", "/tag/", "/author/", "/press", "/top-", "/best-", "/list", "/guide"
    ];

    private static readonly string[] ServiceProviderTerms =
    [
        "web design", "website design", "digital agency", "marketing agency", "seo agency",
        "lead generation", "software platform", "saas", "crm", "directory", "marketplace"
    ];

    private static readonly string[] GenericCompanyTitleTerms =
    [
        "contact", "contact us", "kontakt", "about", "about us", "o nama", "home", "homepage",
        "services", "privacy", "terms", "croatia contact us", "contact croatia", "hrvatska kontakt"
    ];

    public LeadDiscoveryWorkflow(
        LeadGenDbContext db,
        IAiClient aiClient,
        IWebSearchClient searchClient,
        IContactEnrichmentClient contactClient,
        IOptions<LeadGenOptions> options,
        ILogger<LeadDiscoveryWorkflow> logger)
    {
        _db = db;
        _aiClient = aiClient;
        _searchClient = searchClient;
        _contactClient = contactClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IcpProfile> GenerateIcpAsync(Guid campaignId, CancellationToken ct)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(item => item.Id == campaignId, ct)
            ?? throw new KeyNotFoundException("Campaign was not found.");

        var profile = await _aiClient.GenerateJsonAsync<IcpProfile>(new AiRequest(
            "GenerateIcp",
            "Infer the best client segments from the business description and return strict JSON for a detailed, exhaustive ideal customer profile. Do not require pre-defined target customers. Cover every plausible buyer category that could get direct value from this offer, including adjacent segments, decision-maker roles, pains, buying triggers, firmographic clues, public evidence signals, negative signals, and search phrases that would find real prospects. Buyer type entries should be specific and descriptive, not one-word categories. Do not include private personal data.",
            CampaignPrompt(campaign),
            MaxOutputTokens: 3000), ct);

        profile = ClampIcp(profile);
        campaign.IcpJson = JsonSerializer.Serialize(profile, JsonOptions);
        campaign.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return profile;
    }

    public async Task<RunSummary> StartRunAsync(Guid campaignId, int requestedLeadCount, CancellationToken ct)
    {
        var campaignExists = await _db.Campaigns.AnyAsync(item => item.Id == campaignId, ct);
        if (!campaignExists)
        {
            throw new KeyNotFoundException("Campaign was not found.");
        }

        var boundedCount = Math.Clamp(requestedLeadCount <= 0 ? 5 : requestedLeadCount, 1, Math.Max(1, _options.MaxLeadsPerRun));
        var run = new LeadSearchRun
        {
            CampaignId = campaignId,
            RequestedLeadCount = boundedCount,
            Status = LeadSearchRunStatus.Queued
        };

        _db.LeadSearchRuns.Add(run);
        await _db.SaveChangesAsync(ct);
        return await ExecuteRunAsync(run.Id, ct);
    }

    public async Task<RunSummary> ExecuteRunAsync(Guid runId, CancellationToken ct)
    {
        var run = await _db.LeadSearchRuns
            .Include(item => item.Campaign)
            .FirstOrDefaultAsync(item => item.Id == runId, ct)
            ?? throw new KeyNotFoundException("Run was not found.");

        var campaign = run.Campaign ?? throw new InvalidOperationException("Run has no campaign.");
        var logs = ReadLogs(run.LogsJson);

        try
        {
            run.Status = LeadSearchRunStatus.Running;
            run.StartedAtUtc = DateTime.UtcNow;
            run.ErrorMessage = null;
            logs.Add("Queued -> Running");
            logs.Add(GraphEvent("Base", campaign.Name));
            await PersistLogsAsync(run, logs, ct);

            if (string.IsNullOrWhiteSpace(campaign.IcpJson))
            {
                logs.Add("Generate ICP before planning queries");
                await PersistLogsAsync(run, logs, ct);
                var profile = await _aiClient.GenerateJsonAsync<IcpProfile>(new AiRequest(
                    "GenerateIcp",
                    "Infer the best client segments from the business description and return strict JSON for a detailed, exhaustive ideal customer profile. Cover every plausible buyer category that could get direct value from this offer, including adjacent segments, decision-maker roles, pains, buying triggers, firmographic clues, public evidence signals, negative signals, and search phrases. Buyer type entries should be specific and descriptive, not one-word categories.",
                    CampaignPrompt(campaign),
                    MaxOutputTokens: 3000), ct);
                campaign.IcpJson = JsonSerializer.Serialize(ClampIcp(profile), JsonOptions);
                logs.Add("ICP saved for run planning");
                await PersistLogsAsync(run, logs, ct);
            }

            var plan = await _aiClient.GenerateJsonAsync<SearchPlan>(new AiRequest(
                "PlanQueries",
                "Return strict JSON with 5 to 8 search queries that find exact buyer organizations inferred from the ICP, not articles. Use the AI-derived buyer categories, pain points, public signals, and location. Favor queries with public email/phone/contact terms, owner/director/team terms, inquiry terms, and local-language equivalents. Include searches for organizations with weak or missing owned websites by using public profile/contact listing terms only when the result names one exact business. Avoid broad country pages, government trade pages, blogs, best-of lists, agencies/software vendors unless the ICP says they are buyers, LinkedIn-only sources, and login-gated sources.",
                CampaignPrompt(campaign) + Environment.NewLine + "ICP JSON:" + campaign.IcpJson,
                MaxOutputTokens: 800), ct);

            var queries = BuildLeadFocusedQueries(campaign, campaign.IcpJson)
                .Concat(plan.Queries ?? [])
                .Where(item => !string.IsNullOrWhiteSpace(item?.Query))
                .Select(item => item with
                {
                    Query = item.Query.Trim(),
                    Purpose = string.IsNullOrWhiteSpace(item.Purpose) ? "Public web lead search." : item.Purpose.Trim()
                })
                .GroupBy(item => item.Query, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Take(Math.Clamp(_options.MaxSearchQueriesPerRun, 1, 8))
                .ToList();

            run.SearchQueriesJson = JsonSerializer.Serialize(queries, JsonOptions);
            logs.Add($"Generated {queries.Count} bounded search queries");
            foreach (var query in queries)
            {
                logs.Add(GraphEvent("Query", query.Query, query.Purpose));
            }
            await PersistLogsAsync(run, logs, ct);

            var searchResults = new List<SearchResultDto>();
            foreach (var query in queries)
            {
                var results = await _searchClient.SearchAsync(query.Query, Math.Clamp(_options.MaxSearchResultsPerQuery, 1, 10), ct);
                searchResults.AddRange(results);
                logs.Add($"Search returned {results.Count} results for '{query.Query}'");
                foreach (var result in results)
                {
                    var domain = NormalizeDomain(result.Domain, result.Url);
                    if (!string.IsNullOrWhiteSpace(domain))
                    {
                        logs.Add(GraphEvent("Result", query.Query, domain, result.Title, result.Url));
                    }
                }
                await PersistLogsAsync(run, logs, ct);
            }

            var rejectedResults = 0;
            var candidateResults = searchResults
                .Select(result => BuildCandidate(campaign, result))
                .Where(candidate =>
                {
                    var keep = candidate is not null && candidate.Score > 0;
                    if (!keep)
                    {
                        rejectedResults++;
                    }

                    return keep;
                })
                .Select(candidate => candidate!)
                .GroupBy(candidate => candidate.CandidateKey ?? candidate.Domain, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.OrderByDescending(candidate => candidate.Score).First())
                .OrderByDescending(candidate => candidate.Score)
                .Take(Math.Max(run.RequestedLeadCount * 3, Math.Clamp(_options.MaxExtractUrlsPerRun, 1, 80) / 2))
                .ToList();

            logs.Add($"Rejected {rejectedResults} middleman/article/provider results");
            logs.Add($"Deduped to {candidateResults.Count} likely direct company domains");
            foreach (var candidate in candidateResults.Take(40))
            {
                logs.Add(GraphEvent("Candidate", candidate.Domain, candidate.HomePageUrl, candidate.Score.ToString()));
            }

            var extractUrls = candidateResults
                .SelectMany(BuildExtractUrls)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(Math.Clamp(_options.MaxExtractUrlsPerRun, 1, 80))
                .ToList();

            foreach (var candidate in candidateResults.Take(30))
            {
                foreach (var url in BuildExtractUrls(candidate).Take(4))
                {
                    logs.Add(GraphEvent("Extract", candidate.Domain, url));
                }
            }
            await PersistLogsAsync(run, logs, ct);

            var pages = await _searchClient.ExtractAsync(extractUrls, ct);
            logs.Add($"Extracted {pages.Count} public/contact pages from {extractUrls.Count} planned URLs");
            foreach (var page in pages.Take(40))
            {
                var domain = NormalizeDomain(page.Domain, page.Url);
                logs.Add(GraphEvent("Page", domain, page.Title, page.Url));
            }
            await PersistLogsAsync(run, logs, ct);

            var existingLeadIdentities = await _db.Leads
                .Where(lead => lead.CampaignId == campaign.Id)
                .Select(lead => new
                {
                    lead.Domain,
                    lead.DedupeKey,
                    lead.CompanyName,
                    lead.Location
                })
                .ToListAsync(ct);
            var existingKeySet = existingLeadIdentities
                .SelectMany(lead => ExistingKeys(lead.Domain, lead.DedupeKey, lead.CompanyName, lead.Location))
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingDomainSet = existingLeadIdentities
                .Select(lead => LeadIdentity.NormalizeDomain(lead.Domain))
                .Where(domain => !string.IsNullOrWhiteSpace(domain))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var created = 0;
            var skippedWithoutContact = 0;
            var skippedExisting = 0;
            foreach (var candidate in candidateResults)
            {
                if (created >= run.RequestedLeadCount)
                {
                    break;
                }

                var domain = candidate.Domain;
                if (string.IsNullOrWhiteSpace(domain))
                {
                    skippedExisting++;
                    continue;
                }

                if (candidate.IsOwnedWebsite && !existingDomainSet.Add(domain))
                {
                    skippedExisting++;
                    logs.Add(GraphEvent("Duplicate", domain, "Already known company site"));
                    continue;
                }

                var relatedPages = pages
                    .Where(page => string.Equals(NormalizeDomain(page.Domain, page.Url), domain, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (relatedPages.Count == 0)
                {
                    relatedPages.Add(new ExtractedPageDto(candidate.Result.Url, candidate.Result.Title, candidate.Result.Snippet, domain));
                }

                var lead = CreateLead(campaign, run.Id, candidate, relatedPages, created);
                var contacts = await _contactClient.FindContactsAsync(lead.CompanyName, lead.Domain, lead.WebsiteUrl, relatedPages, ct);
                var rankedContacts = contacts
                    .Where(IsPublicContact)
                    .OrderByDescending(ContactRank)
                    .ThenByDescending(contact => contact.ConfidenceScore)
                    .Take(5)
                    .ToList();

                if (!rankedContacts.Any(IsUsableContact) && !HasEnoughPublicEvidence(relatedPages))
                {
                    skippedWithoutContact++;
                    logs.Add(GraphEvent("NoContact", domain, lead.CompanyName));
                    await PersistLogsAsync(run, logs, ct);
                    continue;
                }

                lead.DedupeKey = LeadIdentity.BuildDedupeKey(
                    lead.Domain,
                    lead.CompanyName,
                    lead.Location,
                    rankedContacts.FirstOrDefault(contact => contact.Type == LeadContactType.Email)?.Value);
                var leadKeys = CandidateKeys(lead, rankedContacts).ToList();
                if (leadKeys.Any(existingKeySet.Contains))
                {
                    skippedExisting++;
                    logs.Add(GraphEvent("Duplicate", domain, lead.CompanyName));
                    await PersistLogsAsync(run, logs, ct);
                    continue;
                }

                lead.DossierMarkdown = BuildDossier(campaign, lead, relatedPages, rankedContacts, candidate.IsOwnedWebsite);
                lead.SuggestedOutreachAngle = BuildOutreachAngle(campaign, lead, rankedContacts, candidate.IsOwnedWebsite);
                _db.Leads.Add(lead);
                await _db.SaveChangesAsync(ct);

                foreach (var contact in rankedContacts)
                {
                    _db.LeadContacts.Add(new LeadContact
                    {
                        LeadId = lead.Id,
                        Type = contact.Type,
                        Value = contact.Value,
                        SourceUrl = contact.SourceUrl,
                        ConfidenceScore = Math.Clamp(contact.ConfidenceScore, 0, 100),
                        IsVerified = contact.IsVerified
                    });
                }

                created++;
                foreach (var key in leadKeys)
                {
                    existingKeySet.Add(key);
                }

                var contactSummary = rankedContacts.Any(contact => contact.Type == LeadContactType.Email)
                    ? "email"
                    : rankedContacts.Any(contact => contact.Type == LeadContactType.Phone) ? "phone" : "contact page";
                logs.Add(GraphEvent("Lead", lead.CompanyName, domain, lead.WebsiteUrl, lead.FitScore.ToString()));
                foreach (var contact in rankedContacts)
                {
                    logs.Add(GraphEvent("Contact", lead.CompanyName, contact.Type.ToString(), contact.Value, contact.SourceUrl ?? lead.WebsiteUrl));
                }
                logs.Add($"Saved exact company lead with {contactSummary}: {lead.CompanyName}");
                await PersistLogsAsync(run, logs, ct);
            }

            run.Status = LeadSearchRunStatus.Completed;
            run.CompletedAtUtc = DateTime.UtcNow;
            run.EstimatedCostUsd = Math.Min(_options.MaxRunEstimatedCostUsd, Math.Round(created * 0.0025m, 4));
            if (skippedExisting > 0)
            {
                logs.Add($"Skipped {skippedExisting} existing or duplicate domains");
            }

            if (skippedWithoutContact > 0)
            {
                logs.Add($"Skipped {skippedWithoutContact} likely companies without enough public evidence or contact route");
            }

            logs.Add($"Completed with {created} new leads out of {run.RequestedLeadCount} requested");
            run.LogsJson = JsonSerializer.Serialize(logs, JsonOptions);
            campaign.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return new RunSummary(run.Id, run.Status, created, null);
        }
        catch (Exception ex)
        {
            var safe = SafeError(ex.Message);
            _logger.LogWarning(ex, "Lead discovery run {RunId} failed for campaign {CampaignId}", run.Id, campaign.Id);
            _db.ChangeTracker.Clear();
            var failedRun = await _db.LeadSearchRuns.FirstOrDefaultAsync(item => item.Id == run.Id, CancellationToken.None);
            if (failedRun is not null)
            {
                logs.Add(GraphEvent("Failure", safe));
                logs.Add($"Failed: {safe}");
                failedRun.Status = LeadSearchRunStatus.Failed;
                failedRun.ErrorMessage = safe;
                failedRun.CompletedAtUtc = DateTime.UtcNow;
                failedRun.LogsJson = JsonSerializer.Serialize(logs, JsonOptions);
            }
            await _db.SaveChangesAsync(CancellationToken.None);
            return new RunSummary(run.Id, LeadSearchRunStatus.Failed, 0, safe);
        }
    }

    private async Task PersistLogsAsync(LeadSearchRun run, IReadOnlyList<string> logs, CancellationToken ct)
    {
        run.LogsJson = JsonSerializer.Serialize(logs, JsonOptions);
        await _db.SaveChangesAsync(ct);
    }

    private static List<string> ReadLogs(string? logsJson)
    {
        if (string.IsNullOrWhiteSpace(logsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(logsJson, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string GraphEvent(string type, params string?[] values)
    {
        return "Graph|" + type + "|" + string.Join('|', values.Select(SafeGraphValue));
    }

    private static string SafeGraphValue(string? value)
    {
        var safe = (value ?? string.Empty)
            .Replace("\r", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("\n", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("|", "/", StringComparison.OrdinalIgnoreCase)
            .Trim();
        return safe.Length <= 180 ? safe : safe[..180];
    }

    private static Lead CreateLead(Campaign campaign, Guid runId, LeadCandidate candidate, IReadOnlyList<ExtractedPageDto> pages, int index)
    {
        var result = candidate.Result;
        var domain = candidate.Domain;
        var companyName = CleanCompanyName(result.Title, domain);
        var combinedText = string.Join(' ', pages.Select(page => $"{page.Title} {page.Text}").Append(result.Snippet));
        var whatTheyDo = BestPublicDescription(pages, result.Snippet);
        var location = InferLocation(combinedText, campaign.TargetGeography);
        var industry = InferIndustry(combinedText, campaign);
        var websiteStatus = candidate.IsOwnedWebsite
            ? "Owned website/contact page found"
            : "No owned website verified; saved from a public profile or contact source";
        var whyBuy = BuildWhyBuyReason(campaign, companyName, !candidate.IsOwnedWebsite);
        var evidence = pages.Take(2).Select(page => new
        {
            title = string.IsNullOrWhiteSpace(page.Title) ? companyName : CleanPageTitle(page.Title, page.Url),
            url = page.Url,
            sourceType = candidate.IsOwnedWebsite ? "Owned website" : "Public profile/source",
            websiteStatus,
            quoteOrSummary = CleanEvidenceSummary(string.IsNullOrWhiteSpace(page.Text) ? result.Snippet : page.Text, 260)
        }).ToList();

        var score = Math.Clamp(candidate.Score - index * 2 + (!candidate.IsOwnedWebsite ? 3 : 0), 60, 95);
        var reasons = new[]
        {
            $"Target fit: {companyName} appears to match {AudienceSummary(campaign)} in {location}.",
            whyBuy,
            candidate.IsOwnedWebsite
                ? "Public evidence came from an owned company site or contact page, not an article-only result."
                : "No owned website was verified, which can be a sales angle if the offer includes a free website connected to the system."
        };

        return new Lead
        {
            CampaignId = campaign.Id,
            LeadSearchRunId = runId,
            CompanyName = companyName,
            Domain = candidate.IsOwnedWebsite ? domain : null,
            DedupeKey = LeadIdentity.BuildDedupeKey(candidate.IsOwnedWebsite ? domain : null, companyName, location),
            WebsiteUrl = candidate.IsOwnedWebsite ? candidate.HomePageUrl : result.Url,
            Industry = industry,
            Location = location,
            FitScore = score,
            ConfidenceScore = Math.Clamp(score - 7, 50, 90),
            Status = LeadStatus.New,
            MatchReasonsJson = JsonSerializer.Serialize(reasons, JsonOptions),
            EvidenceJson = JsonSerializer.Serialize(evidence, JsonOptions),
            DossierMarkdown = $"{companyName} appears to be a practical fit for this campaign.",
            SuggestedOutreachAngle = BuildOutreachAngle(campaign, companyName, candidate.IsOwnedWebsite, null)
        };
    }

    internal static IReadOnlyList<SearchPlanQuery> BuildLeadFocusedQueries(Campaign campaign, string? icpJson)
    {
        var geography = FirstUsefulPhrase(campaign.TargetGeography, "local market");
        var buyerSegments = BuyerSegmentsForCampaign(campaign, icpJson).Take(8).ToList();
        var keywords = ReadIcpValues(icpJson, "searchKeywords").Take(4).ToList();
        var exampleQueries = ReadIcpValues(icpJson, "exampleQueries").Take(5);
        var queries = new List<SearchPlanQuery>();

        foreach (var buyerSegment in buyerSegments)
        {
            queries.Add(new SearchPlanQuery($"{buyerSegment} {geography} contact email", "Find exact AI-derived buyer organizations with public contact routes."));
            queries.Add(new SearchPlanQuery($"{buyerSegment} {geography} owner director team contact", "Find public owner, director, or team pages for buyer organizations."));
            queries.Add(new SearchPlanQuery($"{buyerSegment} {geography} inquiry contact", "Find buyer organizations with inquiry or customer-intake contact routes."));
        }

        foreach (var keyword in keywords)
        {
            queries.Add(new SearchPlanQuery($"{keyword} {geography} contact email", "Find ICP-matching organizations using AI-generated search keywords."));
        }

        foreach (var query in exampleQueries)
        {
            queries.Add(new SearchPlanQuery(query, "Use an AI-generated example query from the inferred ICP."));
        }

        return queries
            .Where(query => !string.IsNullOrWhiteSpace(query.Query))
            .GroupBy(query => query.Query, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static IReadOnlyList<string> BuyerSegmentsForCampaign(Campaign campaign, string? icpJson)
    {
        var segments = ReadIcpValues(icpJson, "buyerTypes")
            .Concat(ReadIcpValues(icpJson, "targetIndustries"))
            .Concat(ReadIcpValues(icpJson, "searchKeywords"))
            .Select(ToSearchPhrase)
            .Where(segment => segment.Length is > 2 and <= 90)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();

        return segments.Count == 0
            ? [FirstUsefulPhrase(campaign.BusinessDescription, campaign.BusinessName, "buyer organizations")]
            : segments;
    }

    internal static LeadCandidate? BuildCandidate(Campaign campaign, SearchResultDto result)
    {
        var domain = NormalizeDomain(result.Domain, result.Url);
        if (string.IsNullOrWhiteSpace(domain) || IsBlockedUrl(result.Url) || IsInstitutionalNonLeadDomain(domain))
        {
            return null;
        }

        var homePageUrl = HomePageUrl(result.Url, domain);
        var haystack = $"{result.Title} {result.Snippet} {result.Url}".ToLowerInvariant();
        var isPublicProfile = IsPublicProfileDomain(domain) || LooksLikePublicProfilePath(result.Url);
        if (IsMiddlemanDomain(domain) && !isPublicProfile)
        {
            return null;
        }

        if (IsPublisherOrDirectoryPath(result.Url) || LooksLikeArticleOrDirectory(haystack, isPublicProfile))
        {
            return null;
        }

        if (LooksLikeOwnCompany(campaign, domain) || LooksLikeCompetingProvider(campaign, haystack))
        {
            return null;
        }

        var companyName = CleanCompanyName(result.Title, domain);
        if (IsGenericCompanyTitle(companyName) || LooksLikeNonLeadTitle(companyName, domain))
        {
            return null;
        }

        var score = 70;
        if (haystack.Contains("contact") || haystack.Contains("kontakt") || haystack.Contains("email") || haystack.Contains("@"))
        {
            score += 14;
        }

        if (PathLooksLikeContactPage(result.Url))
        {
            score += 8;
        }

        if (LooksLikeHomePage(result.Url))
        {
            score += 5;
        }

        if (ContainsAny(haystack, AudienceSummary(campaign), campaign.TargetGeography))
        {
            score += 6;
        }

        var buyerMatch = BuyerSegmentsForCampaign(campaign, campaign.IcpJson)
            .Any(segment => haystack.Contains(segment, StringComparison.OrdinalIgnoreCase));
        if (buyerMatch)
        {
            score += 7;
        }

        if (isPublicProfile)
        {
            score += 4;
        }

        var candidateKey = isPublicProfile
            ? $"profile:{LeadIdentity.NormalizeToken(companyName)}:{LeadIdentity.NormalizeToken(InferLocation(haystack, campaign.TargetGeography))}"
            : $"domain:{domain}";

        return new LeadCandidate(result, domain, homePageUrl, Math.Clamp(score, 0, 95), !isPublicProfile, candidateKey);
    }

    internal static IReadOnlyList<string> BuildExtractUrls(LeadCandidate candidate)
    {
        if (!candidate.IsOwnedWebsite)
        {
            return new[] { candidate.Result.Url }
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        var urls = new List<string> { candidate.Result.Url, candidate.HomePageUrl };
        if (Uri.TryCreate(candidate.HomePageUrl, UriKind.Absolute, out var home))
        {
            var root = $"{home.Scheme}://{home.Host}";
            urls.Add($"{root}/contact");
            urls.Add($"{root}/contact-us");
            urls.Add($"{root}/kontakt");
            urls.Add($"{root}/o-nama");
            urls.Add($"{root}/about");
            urls.Add($"{root}/team");
            urls.Add($"{root}/our-team");
            urls.Add($"{root}/staff");
            urls.Add($"{root}/doctors");
            urls.Add($"{root}/nas-tim");
            urls.Add($"{root}/tim");
        }

        return urls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> ReadIcpValues(string? icpJson, params string[] propertyNames)
    {
        if (string.IsNullOrWhiteSpace(icpJson) || propertyNames.Length == 0)
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(icpJson);
            var values = new List<string>();
            foreach (var propertyName in propertyNames)
            {
                if (!document.RootElement.TryGetProperty(propertyName, out var items) || items.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                values.AddRange(items.EnumerateArray()
                    .Select(item => item.GetString())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => item!.Trim()));
            }

            return values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<string> ExistingKeys(string? domain, string? dedupeKey, string? companyName, string? location)
    {
        if (!string.IsNullOrWhiteSpace(dedupeKey))
        {
            yield return dedupeKey;
        }

        var normalizedDomain = LeadIdentity.NormalizeDomain(domain);
        if (!string.IsNullOrWhiteSpace(normalizedDomain))
        {
            yield return $"domain:{normalizedDomain}";
        }

        var nameKey = LeadIdentity.BuildDedupeKey(null, companyName, location);
        if (!string.Equals(nameKey, "name:", StringComparison.OrdinalIgnoreCase))
        {
            yield return nameKey;
        }
    }

    private static IEnumerable<string> CandidateKeys(Lead lead, IReadOnlyList<ContactCandidate> contacts)
    {
        yield return lead.DedupeKey;

        var email = contacts.FirstOrDefault(contact => contact.Type == LeadContactType.Email)?.Value;
        var emailKey = LeadIdentity.BuildDedupeKey(null, lead.CompanyName, lead.Location, email);
        if (!string.IsNullOrWhiteSpace(email) && !string.Equals(emailKey, lead.DedupeKey, StringComparison.OrdinalIgnoreCase))
        {
            yield return emailKey;
        }

        var nameKey = LeadIdentity.BuildDedupeKey(null, lead.CompanyName, lead.Location);
        if (!string.Equals(nameKey, lead.DedupeKey, StringComparison.OrdinalIgnoreCase))
        {
            yield return nameKey;
        }
    }

    private static bool HasEnoughPublicEvidence(IReadOnlyList<ExtractedPageDto> pages)
    {
        return pages.Any(page => CleanEvidenceSummary(page.Text, 180).Length >= 80);
    }

    private static bool IsUsableContact(ContactCandidate contact)
    {
        return contact.Type is LeadContactType.Email or LeadContactType.ContactPage or LeadContactType.Phone;
    }

    private static bool IsPublicContact(ContactCandidate contact)
    {
        return !string.IsNullOrWhiteSpace(contact.Value)
            && contact.Type is LeadContactType.Email or LeadContactType.ContactPage or LeadContactType.Phone or LeadContactType.Social or LeadContactType.Other;
    }

    private static int ContactRank(ContactCandidate contact)
    {
        return contact.Type switch
        {
            LeadContactType.Email => 50,
            LeadContactType.ContactPage => 40,
            LeadContactType.Phone => 30,
            LeadContactType.Social => 20,
            _ => 10
        };
    }

    private static string BuildDossier(Campaign campaign, Lead lead, IReadOnlyList<ExtractedPageDto> pages, IReadOnlyList<ContactCandidate> contacts, bool ownedWebsiteFound)
    {
        var publicDescription = BestPublicDescription(pages, lead.DossierMarkdown);
        var contactSummary = contacts.Count == 0
            ? "No public contact route was saved."
            : string.Join("; ", contacts.Select(FormatContactForDossier));
        var whyBuy = BuildWhyBuyReason(campaign, lead.CompanyName, !ownedWebsiteFound);
        var whyFit = $"Fits the AI-derived target profile of {AudienceSummary(campaign)} in {lead.Location ?? FirstUsefulPhrase(campaign.TargetGeography, "the selected location")}. The lead scored {lead.FitScore} from public evidence, buyer-category signals, and outreach availability.";
        var websiteOpportunity = ownedWebsiteFound
            ? "Owned website or contact page found. Use the outreach to improve booking flow, inquiry capture, local visibility, and follow-up."
            : "No owned website was verified from the saved source. Position the offer as a free website connected to the system so inquiries and bookings are captured in one place.";

        return $"""
        Who they are: {lead.CompanyName} is a public lead in {lead.Location ?? "the selected market"}. Source: {lead.Domain ?? lead.WebsiteUrl ?? "public profile/contact source"}.

        What they do: {publicDescription}

        Why they might buy: {whyBuy}

        Why they are a good fit: {whyFit}

        Website opportunity: {websiteOpportunity}

        Public contact routes found: {contactSummary}

        Recommended next step: verify the source page, confirm the company still matches the ICP, then send a concise audit-style note tied to their booking/contact workflow and the saved contact route.
        """;
    }

    private static string BuildOutreachAngle(Campaign campaign, Lead lead, IReadOnlyList<ContactCandidate> contacts, bool ownedWebsiteFound)
    {
        var primary = contacts.OrderByDescending(ContactRank).FirstOrDefault();
        var route = primary is null ? "their public website" : $"{primary.Type.ToString().ToLowerInvariant()} {primary.Value}";
        return BuildOutreachAngle(campaign, lead.CompanyName, ownedWebsiteFound, route);
    }

    private static string BuildOutreachAngle(Campaign campaign, string companyName, bool ownedWebsiteFound, string? route)
    {
        var offer = ProductOfferSummary(campaign);
        var websiteAngle = ownedWebsiteFound
            ? "improving how website visitors turn into inquiries"
            : "building a free connected website so inquiries do not get lost";
        return $"Reference {companyName}'s public business evidence and {websiteAngle}. Offer {offer}. Use {route ?? "the best saved public contact route"} for manual outreach.";
    }

    private static string BestPublicDescription(IReadOnlyList<ExtractedPageDto> pages, string? fallback = null)
    {
        var source = pages
            .OrderBy(page => PathLooksLikeContactPage(page.Url))
            .ThenByDescending(page => page.Text.Length)
            .FirstOrDefault();
        if (source is null || string.IsNullOrWhiteSpace(source.Text))
        {
            return CleanEvidenceSummary(fallback ?? "The extracted public pages did not include a detailed business description.", 280);
        }

        return CleanEvidenceSummary(source.Text, 280);
    }

    private static string BuildWhyBuyReason(Campaign campaign, string companyName, bool websiteGap)
    {
        var offer = ProductOfferSummary(campaign);
        var painPoint = ReadIcpValues(campaign.IcpJson, "painPoints").FirstOrDefault();
        var buyerNeed = string.IsNullOrWhiteSpace(painPoint)
            ? $"They appear to match the AI-derived buyer profile: {AudienceSummary(campaign)}."
            : $"They likely experience this ICP pain: {painPoint}.";
        var websiteGapText = websiteGap
            ? " A missing or weak owned website makes the free connected website offer especially relevant."
            : " Their existing web presence gives a concrete place to improve conversion and inquiry capture.";
        return $"{buyerNeed}{websiteGapText} The offer to position is {offer} for {companyName}.";
    }

    private static string ProductOfferSummary(Campaign campaign)
    {
        var description = campaign.BusinessDescription;
        if (description.Contains("free website", StringComparison.OrdinalIgnoreCase)
            || description.Contains("website", StringComparison.OrdinalIgnoreCase))
        {
            return "a free website or improved website connected to the lead-management system";
        }

        if (description.Contains("lead", StringComparison.OrdinalIgnoreCase)
            || description.Contains("crm", StringComparison.OrdinalIgnoreCase)
            || description.Contains("system", StringComparison.OrdinalIgnoreCase))
        {
            return "a system that captures, organizes, and follows up with customer inquiries";
        }

        return FirstUsefulPhrase(description, "a focused offer tied to their public business needs");
    }

    private static string AudienceSummary(Campaign? campaign)
    {
        if (campaign is null)
        {
            return "the AI-derived target buyer profile";
        }

        var audience = ReadIcpValues(campaign.IcpJson, "buyerTypes", "targetIndustries")
            .Take(5)
            .ToList();
        if (audience.Count > 0)
        {
            return string.Join(", ", audience);
        }

        return FirstUsefulPhrase(campaign.BusinessDescription, campaign.BusinessName, "the AI-derived target buyer profile");
    }

    private static string CleanPageTitle(string title, string url)
    {
        var clean = CollapseSpaces(title.Replace("\r", " ").Replace("\n", " "));
        if (!string.IsNullOrWhiteSpace(clean) && !IsGenericCompanyTitle(clean))
        {
            return clean.Length <= 140 ? clean : clean[..140];
        }

        return DisplayUrl(url);
    }

    private static string CleanEvidenceSummary(string text, int maxLength)
    {
        var lines = text
            .Replace("\r", "\n", StringComparison.OrdinalIgnoreCase)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => CollapseSpaces(RemoveEmailsAndPhones(line)))
            .Where(line => line.Length >= 12)
            .Where(line => !LooksLikeContactOnlyLine(line))
            .Take(6)
            .ToList();

        var summary = lines.Count == 0
            ? CollapseSpaces(RemoveEmailsAndPhones(text))
            : string.Join(' ', lines);

        summary = summary.Trim(' ', '-', '|', ':');
        if (string.IsNullOrWhiteSpace(summary))
        {
            return "Public pages did not expose a clean business description.";
        }

        return Clip(summary, maxLength);
    }

    private static bool LooksLikeContactOnlyLine(string line)
    {
        var lowered = line.ToLowerInvariant();
        var contactTerms = new[]
        {
            "address", "adresa", "email", "e-mail", "phone", "telefon", "tel.", "fax",
            "map", "office", "kontakt", "contact", "privacy", "cookie", "gdpr"
        };

        return contactTerms.Any(term => lowered.StartsWith(term, StringComparison.Ordinal)
            || lowered.Contains($"{term}:", StringComparison.Ordinal));
    }

    private static string RemoveEmailsAndPhones(string value)
    {
        var withoutEmails = System.Text.RegularExpressions.Regex.Replace(value, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return System.Text.RegularExpressions.Regex.Replace(withoutEmails, @"\+?\d[\d\s()./-]{6,}\d", "");
    }

    private static string DisplayUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url;
        }

        var host = uri.Host.Replace("www.", "", StringComparison.OrdinalIgnoreCase);
        var path = uri.AbsolutePath == "/" ? string.Empty : uri.AbsolutePath.Trim('/');
        return string.IsNullOrWhiteSpace(path) ? host : $"{host}/{path}";
    }

    private static string FormatContactForDossier(ContactCandidate contact)
    {
        return contact.Type switch
        {
            LeadContactType.Email => $"email {contact.Value}",
            LeadContactType.ContactPage => $"contact page {contact.Value}",
            LeadContactType.Phone => $"phone {contact.Value}",
            LeadContactType.Social => $"social profile {contact.Value}",
            _ => contact.Value
        };
    }

    private static bool IsMiddlemanDomain(string domain)
    {
        return MiddlemanDomains.Any(term => domain.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPublicProfileDomain(string domain)
    {
        return PublicProfileDomains.Any(term => domain.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsInstitutionalNonLeadDomain(string domain)
    {
        return InstitutionalNonLeadDomains.Any(term => domain.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPublisherOrDirectoryPath(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var path = uri.AbsolutePath.ToLowerInvariant();
        return PublisherOrDirectoryPathTerms.Any(path.Contains);
    }

    private static bool LooksLikeArticleOrDirectory(string text, bool allowPublicProfile)
    {
        return text.Contains("top 10", StringComparison.OrdinalIgnoreCase)
            || text.Contains("best ", StringComparison.OrdinalIgnoreCase)
            || (!allowPublicProfile && text.Contains("directory", StringComparison.OrdinalIgnoreCase))
            || (!allowPublicProfile && text.Contains("listing", StringComparison.OrdinalIgnoreCase))
            || text.Contains("marketplace", StringComparison.OrdinalIgnoreCase)
            || text.Contains("compare ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikePublicProfilePath(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var path = uri.AbsolutePath.ToLowerInvariant();
        return path.Contains("/profile", StringComparison.Ordinal)
            || path.Contains("/vendor", StringComparison.Ordinal)
            || path.Contains("/venues", StringComparison.Ordinal)
            || path.Contains("/venue", StringComparison.Ordinal)
            || path.Contains("/business", StringComparison.Ordinal)
            || path.Contains("/company", StringComparison.Ordinal)
            || path.Contains("/contact", StringComparison.Ordinal)
            || path.Contains("/kontakt", StringComparison.Ordinal);
    }

    private static bool LooksLikeNonLeadTitle(string companyName, string domain)
    {
        var text = $"{companyName} {domain}".ToLowerInvariant();
        return text.Contains("contact us", StringComparison.Ordinal)
            || text.Contains("embassy", StringComparison.Ordinal)
            || text.Contains("trade office", StringComparison.Ordinal)
            || text.Contains("government", StringComparison.Ordinal)
            || text.Contains("ministry", StringComparison.Ordinal)
            || text.Contains("privacy policy", StringComparison.Ordinal)
            || text.Contains("terms of use", StringComparison.Ordinal);
    }

    private static bool LooksLikeOwnCompany(Campaign campaign, string domain)
    {
        return !string.IsNullOrWhiteSpace(campaign.WebsiteUrl)
            && string.Equals(NormalizeDomain(null, campaign.WebsiteUrl), domain, StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeCompetingProvider(Campaign campaign, string text)
    {
        var businessText = campaign.BusinessDescription.ToLowerInvariant();
        var targetText = AudienceSummary(campaign).ToLowerInvariant();
        var campaignOffersMarketingOrWeb = businessText.Contains("website")
            || businessText.Contains("web design")
            || businessText.Contains("conversion")
            || businessText.Contains("seo")
            || businessText.Contains("marketing")
            || businessText.Contains("lead generation");
        var targetActuallyIsProvider = targetText.Contains("agency")
            || targetText.Contains("marketing")
            || targetText.Contains("software")
            || targetText.Contains("web design");

        return campaignOffersMarketingOrWeb
            && !targetActuallyIsProvider
            && ServiceProviderTerms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool PathLooksLikeContactPage(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var path = uri.AbsolutePath.ToLowerInvariant();
        return path.Contains("contact") || path.Contains("kontakt");
    }

    private static bool LooksLikeHomePage(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.AbsolutePath is "" or "/");
    }

    private static string HomePageUrl(string url, string domain)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return $"{uri.Scheme}://{uri.Host}";
        }

        return $"https://{domain}";
    }

    private static bool ContainsAny(string haystack, params string?[] values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Any(value => haystack.Contains(value!, StringComparison.OrdinalIgnoreCase));
    }

    private static string FirstUsefulPhrase(params string?[] values)
    {
        foreach (var value in values)
        {
            var cleaned = value?
                .Replace("\r", " ", StringComparison.OrdinalIgnoreCase)
                .Replace("\n", " ", StringComparison.OrdinalIgnoreCase)
                .Trim();
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                return cleaned.Length <= 80 ? cleaned : cleaned[..80];
            }
        }

        return "company";
    }

    private static IcpProfile ClampIcp(IcpProfile profile)
    {
        return profile with
        {
            Summary = string.IsNullOrWhiteSpace(profile.Summary) ? "No ICP summary returned." : profile.Summary.Trim(),
            TargetIndustries = TakeClean(profile.TargetIndustries, 20),
            TargetLocations = TakeClean(profile.TargetLocations, 20),
            BuyerTypes = TakeClean(profile.BuyerTypes, 24),
            PainPoints = TakeClean(profile.PainPoints, 24),
            PositiveSignals = TakeClean(profile.PositiveSignals, 24),
            NegativeSignals = TakeClean(profile.NegativeSignals, 16),
            SearchKeywords = TakeClean(profile.SearchKeywords, 24),
            ExampleQueries = TakeClean(profile.ExampleQueries, 10)
        };
    }

    private static string ToSearchPhrase(string value)
    {
        var phrase = value.Trim();
        foreach (var separator in new[] { " - ", " -- ", ": ", " because ", " who ", " that " })
        {
            var index = phrase.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index > 2)
            {
                phrase = phrase[..index];
                break;
            }
        }

        phrase = phrase.Trim(' ', '.', ',', ';', ':', '-', '(', ')');
        return phrase.Length <= 80 ? phrase : phrase[..80].Trim();
    }

    private static IReadOnlyList<string> TakeClean(IEnumerable<string>? values, int count)
    {
        return values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Take(count)
            .ToList()
            ?? [];
    }

    private static string CampaignPrompt(Campaign campaign)
    {
        return $"""
        Business name: {campaign.BusinessName}
        Website: {campaign.WebsiteUrl}
        What the business does: {campaign.BusinessDescription}
        Business location / market: {campaign.TargetGeography}

        Infer the target clients from the business and location. The user does not provide a target-audience list.
        """;
    }

    private static string NormalizeDomain(string? domain, string? url)
    {
        if (!string.IsNullOrWhiteSpace(domain))
        {
            return domain.Trim().Replace("www.", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Host.Replace("www.", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
        }

        return string.Empty;
    }

    private static bool IsBlockedUrl(string url)
    {
        return url.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase)
            || url.Contains("/login", StringComparison.OrdinalIgnoreCase)
            || url.Contains("accounts.", StringComparison.OrdinalIgnoreCase);
    }

    internal static string CleanCompanyName(string title, string domain)
    {
        var name = CollapseSpaces(title.Split('|')[0].Split(" - ", StringSplitOptions.None)[0].Trim(' ', ':', '/', '\\'));
        if (name.Contains(':', StringComparison.Ordinal))
        {
            var beforeColon = CollapseSpaces(name.Split(':')[0]);
            if (beforeColon.Length is >= 3 and <= 80 && !IsGenericCompanyTitle(beforeColon))
            {
                name = beforeColon;
            }
        }

        name = TrimGenericCompanySuffixes(name);
        if (!string.IsNullOrWhiteSpace(name) && name.Length > 2 && !IsGenericCompanyTitle(name))
        {
            return name;
        }

        return CompanyNameFromDomain(domain);
    }

    private static bool IsGenericCompanyTitle(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return GenericCompanyTitleTerms.Any(term => string.Equals(normalized, term, StringComparison.OrdinalIgnoreCase))
            || normalized.EndsWith(" contact us", StringComparison.Ordinal)
            || normalized.EndsWith(" kontakt", StringComparison.Ordinal)
            || normalized.EndsWith(" home", StringComparison.Ordinal);
    }

    private static string TrimGenericCompanySuffixes(string value)
    {
        var suffixes = new[]
        {
            "contact us", "kontakt", "contact", "home", "homepage", "official website", "about us", "o nama"
        };
        var name = value.Trim();
        foreach (var suffix in suffixes)
        {
            if (name.EndsWith(" " + suffix, StringComparison.OrdinalIgnoreCase))
            {
                name = name[..^suffix.Length].Trim(' ', '-', '|', ':');
            }
        }

        return name;
    }

    private static string CompanyNameFromDomain(string domain)
    {
        var firstLabel = domain.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? domain;
        var words = firstLabel
            .Replace("-", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("_", " ", StringComparison.OrdinalIgnoreCase)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(CapitalizeDomainWord);

        var name = string.Join(' ', words);
        return string.IsNullOrWhiteSpace(name) ? domain : name;
    }

    private static string CapitalizeDomainWord(string word)
    {
        return word.Length <= 2
            ? word.ToUpperInvariant()
            : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
    }

    private static string CollapseSpaces(string value)
    {
        return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string InferIndustry(string text, Campaign? campaign = null)
    {
        var combined = $"{text} {AudienceSummary(campaign)} {campaign?.BusinessDescription}";
        if (combined.Contains("event", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("venue", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("wedding", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("conference", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("booking", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("doga", StringComparison.OrdinalIgnoreCase))
        {
            return "Events and venues";
        }

        if (text.Contains("dental", StringComparison.OrdinalIgnoreCase))
        {
            return "Dental clinic";
        }

        if (text.Contains("clinic", StringComparison.OrdinalIgnoreCase) || text.Contains("medical", StringComparison.OrdinalIgnoreCase))
        {
            return "Private healthcare";
        }

        return "Professional services";
    }

    private static string InferLocation(string text, string? fallback = null)
    {
        var known = new[] { "Zagreb", "Split", "Rijeka", "Pula", "Zadar", "Osijek", "Dubrovnik", "Prague", "Warsaw", "Budapest", "Croatia", "Czech Republic", "Poland", "Hungary" };
        var detected = known.FirstOrDefault(item => text.Contains(item, StringComparison.OrdinalIgnoreCase));
        return detected ?? FirstUsefulPhrase(fallback, "Unknown");
    }

    private static string SafeError(string message)
    {
        var safe = message.Replace("\r", " ").Replace("\n", " ");
        return safe[..Math.Min(safe.Length, 500)];
    }

    private static string Clip(string text, int length)
    {
        var clean = text.Replace("\r", " ").Replace("\n", " ");
        return clean.Length <= length ? clean : clean[..length];
    }
}
