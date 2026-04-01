using Leadgen.Lab1Runner.Queries;
using Leadgen.Lab1Runner.Seed;
using Leadgen.Lab1Runner.Services;

var dataset = LeadgenSeedFactory.Create();
var utcNow = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);

PrintHeader("Leadgen Lab 1 Runner");
Console.WriteLine($"Missions: {dataset.Missions.Count}");
Console.WriteLine($"Agents: {dataset.Agents.Count}");
Console.WriteLine($"Companies: {dataset.Missions.SelectMany(mission => mission.Runs).SelectMany(run => run.TargetCompanies).Count()}");
Console.WriteLine($"Contacts: {dataset.Missions.SelectMany(mission => mission.Runs).SelectMany(run => run.TargetCompanies).SelectMany(company => company.Contacts).Count()}");
Console.WriteLine($"Dossiers: {dataset.Missions.SelectMany(mission => mission.Runs).SelectMany(run => run.LeadDossiers).Count()}");

PrintSection("Mission Overview");
foreach (var mission in dataset.Missions)
{
    Console.WriteLine(
        $"- {mission.MissionName} | confidence {mission.ConfidenceScore:F2} | status {mission.Status} | " +
        $"{mission.Runs.Count} run(s) | {mission.ClarificationQuestions.Count(question => !question.IsAnswered)} unanswered question(s)");
}

PrintSection("Query 1 - Missions Below Confidence Threshold");
PrintLines(
    LeadgenQueryCatalog.GetMissionsBelowConfidenceThreshold(dataset)
        .Select(item => $"{item.MissionName} | confidence {item.ConfidenceScore:F2} | status {item.Status} | unanswered {item.UnansweredQuestions}"));

PrintSection("Query 2 - Unanswered Clarification Questions by Slot");
PrintLines(
    LeadgenQueryCatalog.GetUnansweredClarificationQuestionsBySlot(dataset)
        .Select(item => $"{item.SlotName} | unanswered questions {item.QuestionCount}"));

PrintSection("Query 3 - Agent Workload by Role");
PrintLines(
    LeadgenQueryCatalog.GetAgentWorkloadByRole(dataset)
        .Select(item => $"{item.Role} | assignments {item.AssignmentCount} | token budget {item.TotalTokenBudget}"));

PrintSection("Query 4 - Best-Fit Companies");
PrintLines(
    LeadgenQueryCatalog.GetBestFitCompanies(dataset)
        .Select(item => $"{item.CompanyName} ({item.MissionName}) | score {item.MatchScore:F2} | {item.Headquarters} | stage {item.StageLabel}"));

PrintSection("Query 5 - Outreach-Ready Contacts");
PrintLines(
    LeadgenQueryCatalog.GetOutreachReadyContacts(dataset)
        .Select(item => $"{item.ContactName} at {item.CompanyName} | {item.JobTitle} | verified channels {item.VerifiedChannels} | qualification signals {item.QualificationSignals}"));

PrintSection("Query 6 - Top Dossier by Mission");
PrintLines(
    LeadgenQueryCatalog.GetTopDossierByMission(dataset)
        .Select(item => $"{item.MissionName} | {item.ContactName} at {item.CompanyName} | score {item.LeadgenScore} | {item.AdvantagePoint}"));

PrintSection("Query 7 - Evidence Distribution by Kind, Label, and Platform");
PrintLines(
    LeadgenQueryCatalog.GetEvidenceDistribution(dataset)
        .Take(10)
        .Select(item => $"{item.Kind} | {item.Label} | {item.SourcePlatform} | count {item.Count}"));

PrintSection("Query 8 - High-Score Leads Missing Key Contact Channels");
PrintLines(
    LeadgenQueryCatalog.GetHighScoreLeadsMissingKeyChannels(dataset)
        .Select(item => $"{item.ContactName} at {item.CompanyName} | score {item.LeadgenScore} | missing {item.MissingChannel}"));

PrintSection("Query 9 - Recent Qualification Signals");
PrintLines(
    LeadgenQueryCatalog.GetRecentSignals(dataset, utcNow)
        .Take(10)
        .Select(item => $"{item.CapturedAtUtc:yyyy-MM-dd} | {item.ContactName} at {item.CompanyName} | {item.Label} via {item.SourcePlatform}"));

PrintSection("Query 10 - Average Lead Score by Primary Surface");
PrintLines(
    LeadgenQueryCatalog.GetAverageLeadScoreByPrimarySurface(dataset)
        .Select(item => $"{item.GroupKey} | average score {item.AverageScore:F2} | dossiers {item.DossierCount}"));

PrintSection("Async Simulation - Mission A");
var simulationMission = dataset.Missions.First(mission => mission.MissionName.Contains("SQL Optimization", StringComparison.OrdinalIgnoreCase));
var simulator = new MissionResearchSimulator();

try
{
    using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var scoutTask = simulator.RunScoutAsync(simulationMission, cancellationTokenSource.Token);
    var sentinelTask = simulator.RunSentinelAsync(simulationMission, cancellationTokenSource.Token);

    Console.WriteLine("Running Scout and Sentinel in parallel...");
    var companySets = await Task.WhenAll(scoutTask, sentinelTask);

    var shortlistedCompanies = companySets
        .SelectMany(set => set)
        .GroupBy(company => company.Id)
        .Select(group => group.First())
        .OrderByDescending(company => company.MatchScore)
        .ToList();

    Console.WriteLine($"Shortlisted companies: {shortlistedCompanies.Count}");
    foreach (var company in shortlistedCompanies)
    {
        Console.WriteLine($"- {company.Name} | score {company.MatchScore:F2} | signal date {company.LastSignalAtUtc:yyyy-MM-dd}");
    }

    var anchoredContacts = await simulator.RunAnchorAsync(shortlistedCompanies, cancellationTokenSource.Token);
    Console.WriteLine($"Decision makers resolved: {anchoredContacts.Count}");
    foreach (var contact in anchoredContacts)
    {
        Console.WriteLine($"- {contact.FullName} | {contact.JobTitle} | {contact.OpportunitySummary}");
    }

    var qualificationEvidence = await simulator.RunSoulAsync(anchoredContacts, cancellationTokenSource.Token);
    Console.WriteLine($"Qualification evidence extracted: {qualificationEvidence.Count}");
    foreach (var evidence in qualificationEvidence.Take(5))
    {
        Console.WriteLine($"- {evidence.Label} | {evidence.SourcePlatform} | {evidence.Summary}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("The async mission simulation timed out.");
}

static void PrintHeader(string title)
{
    Console.WriteLine(title);
    Console.WriteLine(new string('=', title.Length));
}

static void PrintSection(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
}

static void PrintLines(IEnumerable<string> lines)
{
    var any = false;
    foreach (var line in lines)
    {
        any = true;
        Console.WriteLine($"- {line}");
    }

    if (!any)
    {
        Console.WriteLine("- No results");
    }
}
