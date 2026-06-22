using LeadGen.Core.Domain;
using LeadGen.Core.Providers;
using LeadGen.Infrastructure.Providers;
using LeadGen.Infrastructure.Services;

namespace LeadGen.Tests;

public sealed class LeadQualityFilteringTests
{
    [Fact]
    public void BuildCandidate_RejectsBlogArticleResults()
    {
        var campaign = NewCampaign();
        var result = new SearchResultDto(
            "Top 10 Dental Clinics in Zagreb",
            "https://publisher.example/blog/top-10-dental-clinics-zagreb",
            "A list article comparing clinics.",
            "publisher.example");

        var candidate = LeadDiscoveryWorkflow.BuildCandidate(campaign, result);

        Assert.Null(candidate);
    }

    [Fact]
    public void BuildCandidate_RejectsCompetingServiceProviders()
    {
        var campaign = NewCampaign();
        var result = new SearchResultDto(
            "Dental Marketing Agency Croatia",
            "https://agency.example/contact",
            "SEO agency and web design services for dental clinics.",
            "agency.example");

        var candidate = LeadDiscoveryWorkflow.BuildCandidate(campaign, result);

        Assert.Null(candidate);
    }

    [Fact]
    public void BuildCandidate_AcceptsDirectCompanyContactPages()
    {
        var campaign = NewCampaign();
        var result = new SearchResultDto(
            "Smile Clinic Zagreb Kontakt",
            "https://smile-clinic.example/kontakt",
            "Dental clinic in Zagreb. Contact email and appointment information.",
            "smile-clinic.example");

        var candidate = LeadDiscoveryWorkflow.BuildCandidate(campaign, result);

        Assert.NotNull(candidate);
        Assert.Equal("smile-clinic.example", candidate.Domain);
        Assert.Equal("https://smile-clinic.example", candidate.HomePageUrl);
        Assert.True(candidate.Score >= 90);
    }

    [Fact]
    public void BuildCandidate_RejectsGovernmentContactPages()
    {
        var campaign = NewCampaign();
        var result = new SearchResultDto(
            "Croatia Contact Us",
            "https://www.trade.gov/croatia-contact-us",
            "Government trade office contact information.",
            "trade.gov");

        var candidate = LeadDiscoveryWorkflow.BuildCandidate(campaign, result);

        Assert.Null(candidate);
    }

    [Fact]
    public void BuildCandidate_AcceptsExactPublicProfileAsWebsiteGap()
    {
        var campaign = NewEventCampaign();
        var result = new SearchResultDto(
            "Blue Hall Zagreb Wedding Venue",
            "https://eventective.example/venues/blue-hall-zagreb",
            "Blue Hall is a wedding and conference venue in Zagreb. Contact the venue for booking inquiries.",
            "eventective.example");

        var candidate = LeadDiscoveryWorkflow.BuildCandidate(campaign, result);

        Assert.NotNull(candidate);
        Assert.False(candidate.IsOwnedWebsite);
        Assert.Contains("profile:", candidate.CandidateKey);
    }

    [Fact]
    public void BuildExtractUrls_IncludesHomeAndContactPages()
    {
        var candidate = new LeadCandidate(
            new SearchResultDto("Smile Clinic", "https://smile-clinic.example/kontakt", "", "smile-clinic.example"),
            "smile-clinic.example",
            "https://smile-clinic.example",
            90);

        var urls = LeadDiscoveryWorkflow.BuildExtractUrls(candidate);

        Assert.Contains("https://smile-clinic.example", urls);
        Assert.Contains("https://smile-clinic.example/contact", urls);
        Assert.Contains("https://smile-clinic.example/kontakt", urls);
        Assert.Contains("https://smile-clinic.example/team", urls);
        Assert.Contains("https://smile-clinic.example/o-nama", urls);
    }

    [Fact]
    public void BuildLeadFocusedQueries_IncludesContactAndEmailTerms()
    {
        var queries = LeadDiscoveryWorkflow.BuildLeadFocusedQueries(NewCampaign(), IcpJson(
            ["Dental clinics", "Private healthcare practices"],
            ["Dental care", "Private healthcare"],
            ["clinic website", "private practice"]));

        Assert.Contains(queries, query => query.Query.Contains("email", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(queries, query => query.Query.Contains("contact", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(queries, query => query.Query.Contains("team", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildLeadFocusedQueries_UsesIcpBuyerSegmentsAndLocation()
    {
        var queries = LeadDiscoveryWorkflow.BuildLeadFocusedQueries(NewEventCampaign(), IcpJson(
            ["event venues", "wedding halls", "restaurants with private event rooms"],
            ["Events", "Hospitality"],
            ["venue booking", "event services"]));

        Assert.Contains(queries, query => query.Query.Contains("Zagreb", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(queries, query => query.Query.Contains("event venues", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(queries, query => query.Query.Contains("wedding halls", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildLeadFocusedQueries_CleansDetailedBuyerDescriptions()
    {
        var queries = LeadDiscoveryWorkflow.BuildLeadFocusedQueries(NewEventCampaign(), IcpJson(
            ["Wedding halls and banquet venues - need more direct bookings and better inquiry capture"],
            ["Events and hospitality"],
            ["venue booking"]));

        Assert.Contains(queries, query => query.Query.Contains("Wedding halls and banquet venues Zagreb contact email", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(queries, query => query.Query.Contains("need more direct bookings", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CleanCompanyName_UsesDomainWhenTitleIsGenericContactPage()
    {
        var name = LeadDiscoveryWorkflow.CleanCompanyName("Contact", "dental-center-croatia.com");

        Assert.Equal("Dental Center Croatia", name);
    }

    [Fact]
    public void CleanCompanyName_TrimsColonDescription()
    {
        var name = LeadDiscoveryWorkflow.CleanCompanyName(
            "Creative Pro Group: Europska agencija za organizaciju dogadaja",
            "creativepro.hr");

        Assert.Equal("Creative Pro Group", name);
    }

    [Fact]
    public void LeadIdentity_BuildsStableDomainDedupeKey()
    {
        var first = LeadIdentity.BuildDedupeKey("https://www.creativepro.hr/contact", "Creative Pro Group", "Zagreb");
        var second = LeadIdentity.BuildDedupeKey("creativepro.hr", "Creative Pro Group", "Zagreb");

        Assert.Equal("domain:creativepro.hr", first);
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task PublicContactEnrichment_ReturnsEmailsBeforeContactPages()
    {
        var client = new PublicContactEnrichmentClient();
        var pages = new[]
        {
            new ExtractedPageDto(
                "https://smile-clinic.example/kontakt",
                "Kontakt",
                "Kontaktirajte nas na info@smile-clinic.example za narudzbe.",
                "smile-clinic.example")
        };

        var contacts = await client.FindContactsAsync("Smile Clinic", "smile-clinic.example", "https://smile-clinic.example", pages, CancellationToken.None);

        Assert.NotEmpty(contacts);
        Assert.Equal(LeadContactType.Email, contacts[0].Type);
        Assert.Equal("info@smile-clinic.example", contacts[0].Value);
        Assert.Equal("https://smile-clinic.example/kontakt", contacts[0].SourceUrl);
    }

    [Fact]
    public async Task PublicContactEnrichment_ReturnsPhonesAndPublicPeople()
    {
        var client = new PublicContactEnrichmentClient();
        var pages = new[]
        {
            new ExtractedPageDto(
                "https://smile-clinic.example/o-nama",
                "O nama",
                "Our dentist Dr Mark Stone leads the clinic team. Kontakt telefon +385 1 234 5678.",
                "smile-clinic.example")
        };

        var contacts = await client.FindContactsAsync("Smile Clinic", "smile-clinic.example", "https://smile-clinic.example", pages, CancellationToken.None);

        Assert.Contains(contacts, contact => contact.Type == LeadContactType.Phone && contact.Value.Contains("+385", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(contacts, contact => contact.Type == LeadContactType.Other && contact.Value.Contains("Mark Stone", StringComparison.OrdinalIgnoreCase));
    }

    private static Campaign NewCampaign()
    {
        return new Campaign
        {
            Name = "Dental campaign",
            BusinessName = "Conversion Studio",
            WebsiteUrl = "https://conversion-studio.example",
            BusinessDescription = "We build conversion-focused websites and local SEO campaigns for private clinics.",
            TargetGeography = "Croatia",
            TargetCustomers = "Dental clinics and private healthcare practices",
            Exclusions = "No LinkedIn-only, directories, articles, or agencies"
        };
    }

    private static Campaign NewEventCampaign()
    {
        return new Campaign
        {
            Name = "Eventspace campaign",
            BusinessName = "Eventspace",
            WebsiteUrl = "https://eventspace.example",
            BusinessDescription = "We offer free website building connected to a lead and booking system for event businesses.",
            TargetGeography = "Zagreb",
            TargetCustomers = "Event venues, wedding planners, conference spaces",
            Exclusions = "No LinkedIn-only, broad directories, articles, or government trade pages"
        };
    }

    private static string IcpJson(string[] buyerTypes, string[] targetIndustries, string[] searchKeywords)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            summary = "AI-derived target profile.",
            buyerTypes,
            targetIndustries,
            targetLocations = new[] { "Croatia" },
            painPoints = new[] { "Needs more qualified inquiries." },
            positiveSignals = new[] { "Has public contact or booking pages." },
            negativeSignals = new[] { "Not a direct buyer." },
            searchKeywords,
            exampleQueries = searchKeywords.Select(keyword => $"{keyword} Croatia contact email").ToArray()
        }, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
    }
}
