using Leadgen.Lab1Runner.Queries;
using Leadgen.Model.Enums;
using leadgen.ViewModels.Home;

namespace leadgen.Services;

public sealed class LeadgenDashboardService : ILeadgenDashboardService
{
    private readonly ILeadgenReadRepository _repository;

    public LeadgenDashboardService(ILeadgenReadRepository repository)
    {
        _repository = repository;
    }

    public HomeDashboardViewModel BuildDashboard()
    {
        var snapshot = BuildSnapshot();
        var leadMission = snapshot.FeaturedMissions.FirstOrDefault();

        return new HomeDashboardViewModel
        {
            MissionCount = snapshot.MissionCount,
            AgentCount = snapshot.AgentCount,
            CompanyCount = snapshot.CompanyCount,
            ContactCount = snapshot.ContactCount,
            DossierCount = snapshot.DossierCount,
            FeaturedMissions = snapshot.FeaturedMissions,
            TopLeads = snapshot.TopLeads,
            RecentSignals = snapshot.RecentSignals,
            SurfaceScores = snapshot.SurfaceScores,
            EntityNavigation = snapshot.EntityNavigation,
            SwarmNodes = snapshot.SwarmNodes,
            SuggestedMissionDna = leadMission is null ? string.Empty : $"{leadMission.ProductName} for {leadMission.Persona}"
        };
    }

    public MissionCanvasViewModel BuildMissionCanvas(string? missionDna)
    {
        var snapshot = BuildSnapshot();
        var leadMission = snapshot.FeaturedMissions.FirstOrDefault();
        var resolvedMissionDna = string.IsNullOrWhiteSpace(missionDna)
            ? leadMission is null ? "B2B lead qualification" : $"{leadMission.ProductName} for {leadMission.Persona}"
            : missionDna.Trim();

        return new MissionCanvasViewModel
        {
            SubmittedMissionDna = resolvedMissionDna,
            OperationLabel = "B2B Resolution",
            MissionName = leadMission?.MissionName ?? "Leadgen Mission",
            MissionStatus = leadMission?.Status ?? "Queued",
            ConfidenceScore = leadMission?.ConfidenceScore ?? 0.92m,
            StartingProgress = 14,
            AgentCount = snapshot.AgentCount,
            CompanyCount = snapshot.CompanyCount,
            ContactCount = snapshot.ContactCount,
            DossierCount = snapshot.DossierCount,
            SwarmNodes = snapshot.SwarmNodes,
            TopLeads = snapshot.TopLeads,
            RecentSignals = snapshot.RecentSignals,
            EntityNavigation = snapshot.EntityNavigation
        };
    }

    private DashboardSnapshot BuildSnapshot()
    {
        var dataset = _repository.GetDataset();
        var companies = dataset.Missions.SelectMany(mission => mission.Runs).SelectMany(run => run.TargetCompanies).ToList();
        var contacts = companies.SelectMany(company => company.Contacts).ToList();
        var questions = dataset.Missions.SelectMany(mission => mission.ClarificationQuestions).ToList();
        var runs = dataset.Missions.SelectMany(mission => mission.Runs).ToList();
        var assignments = runs.SelectMany(run => run.AgentAssignments).ToList();
        var channels = contacts.SelectMany(contact => contact.ContactChannels).ToList();
        var evidencePoints = contacts.SelectMany(contact => contact.EvidencePoints).ToList();
        var dossiers = dataset.Missions.SelectMany(mission => mission.Runs).SelectMany(run => run.LeadDossiers).ToList();
        var mostRecentSignalAtUtc = evidencePoints
            .Select(evidence => evidence.CapturedAtUtc)
            .DefaultIfEmpty(new DateTime(2026, 4, 16, 12, 0, 0, DateTimeKind.Utc))
            .Max();

        return new DashboardSnapshot
        {
            MissionCount = dataset.Missions.Count,
            AgentCount = dataset.Agents.Count,
            CompanyCount = companies.Count,
            ContactCount = contacts.Count,
            DossierCount = dossiers.Count,
            FeaturedMissions = dataset.Missions
                .OrderByDescending(mission => mission.ConfidenceScore)
                .Select(mission => new DashboardMissionCardViewModel
                {
                    MissionId = mission.Id,
                    MissionName = mission.MissionName,
                    ProductName = mission.ProductName,
                    Status = mission.Status.ToString(),
                    ConfidenceScore = mission.ConfidenceScore,
                    Persona = mission.Persona,
                    Surface = mission.PrimarySurface,
                    RunCount = mission.Runs.Count
                })
                .ToList(),
            TopLeads = LeadgenQueryCatalog.GetTopDossierByMission(dataset)
                .Take(3)
                .Select(item => new DashboardLeadViewModel
                {
                    MissionName = item.MissionName,
                    CompanyName = item.CompanyName,
                    ContactName = item.ContactName,
                    LeadgenScore = item.LeadgenScore,
                    AdvantagePoint = item.AdvantagePoint
                })
                .ToList(),
            RecentSignals = LeadgenQueryCatalog.GetRecentSignals(dataset, mostRecentSignalAtUtc.AddDays(1))
                .Take(4)
                .Select(item => new DashboardSignalViewModel
                {
                    MissionName = item.MissionName,
                    CompanyName = item.CompanyName,
                    ContactName = item.ContactName,
                    Label = item.Label,
                    SourcePlatform = item.SourcePlatform,
                    CapturedAtUtc = item.CapturedAtUtc
                })
                .ToList(),
            SurfaceScores = LeadgenQueryCatalog.GetAverageLeadScoreByPrimarySurface(dataset)
                .Take(3)
                .Select(item => new DashboardSurfaceScoreViewModel
                {
                    Surface = item.GroupKey,
                    AverageScore = item.AverageScore,
                    DossierCount = item.DossierCount
                })
                .ToList(),
            EntityNavigation =
            [
                new DashboardEntityLinkViewModel { Label = "Missions", Description = "Business DNA intake, readiness, and mission states.", Controller = "Missions", Count = dataset.Missions.Count },
                new DashboardEntityLinkViewModel { Label = "Questions", Description = "Clarification loops used to sharpen mission confidence.", Controller = "ClarificationQuestions", Count = questions.Count },
                new DashboardEntityLinkViewModel { Label = "Runs", Description = "Execution instances of swarm research against a mission.", Controller = "MissionRuns", Count = runs.Count },
                new DashboardEntityLinkViewModel { Label = "Assignments", Description = "Who did what inside each mission run.", Controller = "MissionAgentAssignments", Count = assignments.Count },
                new DashboardEntityLinkViewModel { Label = "Agents", Description = "Specialized swarm roles and their active operating context.", Controller = "SwarmAgents", Count = dataset.Agents.Count },
                new DashboardEntityLinkViewModel { Label = "Companies", Description = "Shortlisted organizations qualified against mission fit.", Controller = "TargetCompanies", Count = companies.Count },
                new DashboardEntityLinkViewModel { Label = "Contacts", Description = "Decision-makers and supporting outreach context.", Controller = "TargetContacts", Count = contacts.Count },
                new DashboardEntityLinkViewModel { Label = "Channels", Description = "Verified contact vectors for each target person.", Controller = "ContactChannels", Count = channels.Count },
                new DashboardEntityLinkViewModel { Label = "Evidence", Description = "Signals, proof points, and traceable qualification context.", Controller = "EvidencePoints", Count = evidencePoints.Count },
                new DashboardEntityLinkViewModel { Label = "Dossiers", Description = "Final lead outputs with score and outreach angle.", Controller = "LeadDossiers", Count = dossiers.Count }
            ],
            SwarmNodes = dataset.Agents
                .OrderBy(agent => GetNodeOrder(agent.Role))
                .Select(agent =>
                {
                    var coordinates = GetNodeCoordinates(agent.Role);

                    return new DashboardAgentNodeViewModel
                    {
                        AgentId = agent.Id,
                        Role = agent.Role.ToString(),
                        CodeName = agent.CodeName,
                        Provider = agent.Provider,
                        CurrentFocus = agent.CurrentFocus,
                        MaxConcurrentTasks = agent.MaxConcurrentTasks,
                        IsActive = agent.IsActive,
                        Column = coordinates.column,
                        Row = coordinates.row
                    };
                })
                .ToList()
        };
    }

    private static int GetNodeOrder(AgentRole role) =>
        role switch
        {
            AgentRole.Strategist => 0,
            AgentRole.Scout => 1,
            AgentRole.Sentinel => 2,
            AgentRole.Anchor => 3,
            AgentRole.Soul => 4,
            AgentRole.Stitcher => 5,
            AgentRole.Sniper => 6,
            _ => 99
        };

    private static (int column, int row) GetNodeCoordinates(AgentRole role) =>
        role switch
        {
            AgentRole.Strategist => (3, 2),
            AgentRole.Scout => (2, 1),
            AgentRole.Sentinel => (4, 1),
            AgentRole.Anchor => (3, 4),
            AgentRole.Soul => (1, 1),
            AgentRole.Stitcher => (2, 5),
            AgentRole.Sniper => (5, 1),
            _ => (3, 3)
        };

    private sealed class DashboardSnapshot
    {
        public int MissionCount { get; init; }

        public int AgentCount { get; init; }

        public int CompanyCount { get; init; }

        public int ContactCount { get; init; }

        public int DossierCount { get; init; }

        public IReadOnlyList<DashboardMissionCardViewModel> FeaturedMissions { get; init; } = Array.Empty<DashboardMissionCardViewModel>();

        public IReadOnlyList<DashboardLeadViewModel> TopLeads { get; init; } = Array.Empty<DashboardLeadViewModel>();

        public IReadOnlyList<DashboardSignalViewModel> RecentSignals { get; init; } = Array.Empty<DashboardSignalViewModel>();

        public IReadOnlyList<DashboardSurfaceScoreViewModel> SurfaceScores { get; init; } = Array.Empty<DashboardSurfaceScoreViewModel>();

        public IReadOnlyList<DashboardEntityLinkViewModel> EntityNavigation { get; init; } = Array.Empty<DashboardEntityLinkViewModel>();

        public IReadOnlyList<DashboardAgentNodeViewModel> SwarmNodes { get; init; } = Array.Empty<DashboardAgentNodeViewModel>();
    }
}
