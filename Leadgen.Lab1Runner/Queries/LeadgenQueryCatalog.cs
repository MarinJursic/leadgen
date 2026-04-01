using Leadgen.Lab1Runner.Seed;
using Leadgen.Model.Entities;
using Leadgen.Model.Enums;

namespace Leadgen.Lab1Runner.Queries;

public static class LeadgenQueryCatalog
{
    public static IEnumerable<MissionReadinessSummary> GetMissionsBelowConfidenceThreshold(
        LeadgenLabDataset dataset,
        decimal threshold = 0.80m)
    {
        return dataset.Missions
            .Where(mission => mission.ConfidenceScore < threshold)
            .OrderBy(mission => mission.ConfidenceScore)
            .Select(mission => new MissionReadinessSummary(
                mission.MissionName,
                mission.ConfidenceScore,
                mission.Status,
                mission.ClarificationQuestions.Count(question => !question.IsAnswered)));
    }

    public static IEnumerable<SlotQuestionSummary> GetUnansweredClarificationQuestionsBySlot(LeadgenLabDataset dataset)
    {
        return dataset.Missions
            .SelectMany(mission => mission.ClarificationQuestions)
            .Where(question => !question.IsAnswered)
            .GroupBy(question => question.SlotName)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => new SlotQuestionSummary(group.Key, group.Count()));
    }

    public static IEnumerable<AgentWorkloadSummary> GetAgentWorkloadByRole(LeadgenLabDataset dataset)
    {
        var agentsById = dataset.Agents.ToDictionary(agent => agent.Id);

        return dataset.Missions
            .SelectMany(mission => mission.Runs)
            .SelectMany(run => run.AgentAssignments)
            .Where(assignment => agentsById.TryGetValue(assignment.SwarmAgentId, out var agent) && agent.IsActive)
            .GroupBy(assignment => agentsById[assignment.SwarmAgentId].Role)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => new AgentWorkloadSummary(
                group.Key,
                group.Count(),
                group.Sum(assignment => assignment.TokenBudget)));
    }

    public static IEnumerable<CompanyFitSummary> GetBestFitCompanies(
        LeadgenLabDataset dataset,
        decimal minimumMatchScore = 0.85m,
        int take = 5)
    {
        return EnumerateCompanies(dataset)
            .Where(item => item.Company.IsHeadquartersVerified && item.Company.MatchScore >= minimumMatchScore)
            .OrderByDescending(item => item.Company.MatchScore)
            .Take(take)
            .Select(item => new CompanyFitSummary(
                item.Mission.MissionName,
                item.Company.Name,
                item.Company.MatchScore,
                $"{item.Company.HeadquartersCity}, {item.Company.HeadquartersCountry}",
                item.Company.OrganizationStageLabel ?? "Unspecified"));
    }

    public static IEnumerable<OutreachReadyContactSummary> GetOutreachReadyContacts(LeadgenLabDataset dataset)
    {
        return EnumerateContacts(dataset)
            .Select(item => new
            {
                item.Mission,
                item.Company,
                item.Contact,
                VerifiedChannels = item.Contact.ContactChannels.Count(channel =>
                    channel.IsVerified &&
                    (channel.Type == ContactChannelType.WorkEmail || channel.Type == ContactChannelType.Phone)),
                QualificationSignals = item.Contact.EvidencePoints.Count(evidence => evidence.IsQualificationSignal)
            })
            .Where(item => item.VerifiedChannels > 0 && item.QualificationSignals > 0)
            .OrderByDescending(item => item.QualificationSignals)
            .ThenByDescending(item => item.VerifiedChannels)
            .Select(item => new OutreachReadyContactSummary(
                item.Mission.MissionName,
                item.Company.Name,
                item.Contact.FullName,
                item.Contact.JobTitle,
                item.VerifiedChannels,
                item.QualificationSignals));
    }

    public static IEnumerable<TopDossierSummary> GetTopDossierByMission(LeadgenLabDataset dataset)
    {
        return EnumerateDossiers(dataset)
            .GroupBy(item => item.Mission.Id)
            .Select(group => group.OrderByDescending(item => item.Dossier.LeadgenScore).First())
            .OrderByDescending(item => item.Dossier.LeadgenScore)
            .Select(item => new TopDossierSummary(
                item.Mission.MissionName,
                item.Company.Name,
                item.Contact.FullName,
                item.Dossier.LeadgenScore,
                item.Dossier.AdvantagePoint));
    }

    public static IEnumerable<EvidenceDistributionSummary> GetEvidenceDistribution(LeadgenLabDataset dataset)
    {
        return EnumerateEvidence(dataset)
            .GroupBy(item => new { item.Evidence.Kind, item.Evidence.Label, item.Evidence.SourcePlatform })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.Kind)
            .ThenBy(group => group.Key.Label)
            .Select(group => new EvidenceDistributionSummary(
                group.Key.Kind,
                group.Key.Label,
                group.Key.SourcePlatform,
                group.Count()));
    }

    public static IEnumerable<MissingChannelSummary> GetHighScoreLeadsMissingKeyChannels(
        LeadgenLabDataset dataset,
        int minimumLeadScore = 8)
    {
        return EnumerateDossiers(dataset)
            .Where(item => item.Dossier.LeadgenScore >= minimumLeadScore)
            .SelectMany(item =>
            {
                var missingChannels = new List<string>();

                if (!item.Contact.ContactChannels.Any(channel => channel.IsVerified && channel.Type == ContactChannelType.WorkEmail))
                {
                    missingChannels.Add("Verified work email");
                }

                if (!item.Contact.ContactChannels.Any(channel => channel.IsVerified && channel.Type == ContactChannelType.Phone))
                {
                    missingChannels.Add("Verified phone");
                }

                return missingChannels.Select(missingChannel => new MissingChannelSummary(
                    item.Mission.MissionName,
                    item.Company.Name,
                    item.Contact.FullName,
                    item.Dossier.LeadgenScore,
                    missingChannel));
            })
            .OrderByDescending(item => item.LeadgenScore)
            .ThenBy(item => item.ContactName);
    }

    public static IEnumerable<RecentSignalSummary> GetRecentSignals(
        LeadgenLabDataset dataset,
        DateTime utcNow,
        int days = 30)
    {
        var cutoff = utcNow.AddDays(-days);

        return EnumerateEvidence(dataset)
            .Where(item => item.Evidence.IsQualificationSignal && item.Evidence.CapturedAtUtc >= cutoff)
            .OrderByDescending(item => item.Evidence.CapturedAtUtc)
            .Select(item => new RecentSignalSummary(
                item.Mission.MissionName,
                item.Company.Name,
                item.Contact.FullName,
                item.Evidence.Label,
                item.Evidence.SourcePlatform,
                item.Evidence.CapturedAtUtc));
    }

    public static IEnumerable<AverageLeadScoreSummary> GetAverageLeadScoreByPrimarySurface(LeadgenLabDataset dataset)
    {
        return EnumerateDossiers(dataset)
            .GroupBy(item => item.Mission.PrimarySurface)
            .OrderByDescending(group => group.Average(item => item.Dossier.LeadgenScore))
            .Select(group => new AverageLeadScoreSummary(
                group.Key,
                Math.Round(group.Average(item => item.Dossier.LeadgenScore), 2),
                group.Count()));
    }

    private static IEnumerable<(BusinessDnaMission Mission, MissionRun Run, TargetCompany Company)> EnumerateCompanies(LeadgenLabDataset dataset)
    {
        return dataset.Missions
            .SelectMany(mission => mission.Runs.SelectMany(run => run.TargetCompanies.Select(company => (mission, run, company))));
    }

    private static IEnumerable<(BusinessDnaMission Mission, MissionRun Run, TargetCompany Company, TargetContact Contact)> EnumerateContacts(LeadgenLabDataset dataset)
    {
        return EnumerateCompanies(dataset)
            .SelectMany(item => item.Company.Contacts.Select(contact => (item.Mission, item.Run, item.Company, contact)));
    }

    private static IEnumerable<(BusinessDnaMission Mission, MissionRun Run, TargetCompany Company, TargetContact Contact, LeadDossier Dossier)> EnumerateDossiers(LeadgenLabDataset dataset)
    {
        foreach (var mission in dataset.Missions)
        {
            foreach (var run in mission.Runs)
            {
                foreach (var dossier in run.LeadDossiers)
                {
                    var company = run.TargetCompanies.FirstOrDefault(targetCompany => targetCompany.Id == dossier.TargetCompanyId);
                    if (company is null)
                    {
                        continue;
                    }

                    var contact = company.Contacts.FirstOrDefault(targetContact => targetContact.Id == dossier.TargetContactId);
                    if (contact is null)
                    {
                        continue;
                    }

                    yield return (mission, run, company, contact, dossier);
                }
            }
        }
    }

    private static IEnumerable<(BusinessDnaMission Mission, TargetCompany Company, TargetContact Contact, EvidencePoint Evidence)> EnumerateEvidence(LeadgenLabDataset dataset)
    {
        return EnumerateContacts(dataset)
            .SelectMany(item => item.Contact.EvidencePoints.Select(evidence => (item.Mission, item.Company, item.Contact, evidence)));
    }
}

public sealed record MissionReadinessSummary(
    string MissionName,
    decimal ConfidenceScore,
    MissionStatus Status,
    int UnansweredQuestions);

public sealed record SlotQuestionSummary(string SlotName, int QuestionCount);

public sealed record AgentWorkloadSummary(AgentRole Role, int AssignmentCount, int TotalTokenBudget);

public sealed record CompanyFitSummary(
    string MissionName,
    string CompanyName,
    decimal MatchScore,
    string Headquarters,
    string StageLabel);

public sealed record OutreachReadyContactSummary(
    string MissionName,
    string CompanyName,
    string ContactName,
    string JobTitle,
    int VerifiedChannels,
    int QualificationSignals);

public sealed record TopDossierSummary(
    string MissionName,
    string CompanyName,
    string ContactName,
    int LeadgenScore,
    string AdvantagePoint);

public sealed record EvidenceDistributionSummary(
    EvidenceKind Kind,
    string Label,
    string SourcePlatform,
    int Count);

public sealed record MissingChannelSummary(
    string MissionName,
    string CompanyName,
    string ContactName,
    int LeadgenScore,
    string MissingChannel);

public sealed record RecentSignalSummary(
    string MissionName,
    string CompanyName,
    string ContactName,
    string Label,
    string SourcePlatform,
    DateTime CapturedAtUtc);

public sealed record AverageLeadScoreSummary(string GroupKey, double AverageScore, int DossierCount);
