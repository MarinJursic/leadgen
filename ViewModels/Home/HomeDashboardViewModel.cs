namespace leadgen.ViewModels.Home;

public sealed class HomeDashboardViewModel
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

    public string SuggestedMissionDna { get; init; } = string.Empty;
}

public sealed class DashboardMissionCardViewModel
{
    public Guid MissionId { get; init; }

    public string MissionName { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public decimal ConfidenceScore { get; init; }

    public string Persona { get; init; } = string.Empty;

    public string Surface { get; init; } = string.Empty;

    public int RunCount { get; init; }
}

public sealed class DashboardLeadViewModel
{
    public string MissionName { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string ContactName { get; init; } = string.Empty;

    public int LeadgenScore { get; init; }

    public string AdvantagePoint { get; init; } = string.Empty;
}

public sealed class DashboardSignalViewModel
{
    public string MissionName { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string ContactName { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string SourcePlatform { get; init; } = string.Empty;

    public DateTime CapturedAtUtc { get; init; }
}

public sealed class DashboardSurfaceScoreViewModel
{
    public string Surface { get; init; } = string.Empty;

    public double AverageScore { get; init; }

    public int DossierCount { get; init; }
}

public sealed class DashboardEntityLinkViewModel
{
    public string Label { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Controller { get; init; } = string.Empty;

    public int Count { get; init; }
}

public sealed class DashboardAgentNodeViewModel
{
    public Guid AgentId { get; init; }

    public string Role { get; init; } = string.Empty;

    public string CodeName { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;

    public string CurrentFocus { get; init; } = string.Empty;

    public int MaxConcurrentTasks { get; init; }

    public bool IsActive { get; init; }

    public int Column { get; init; }

    public int Row { get; init; }
}

public sealed class MissionCanvasViewModel
{
    public string SubmittedMissionDna { get; init; } = string.Empty;

    public string OperationLabel { get; init; } = string.Empty;

    public string MissionName { get; init; } = string.Empty;

    public string MissionStatus { get; init; } = string.Empty;

    public decimal ConfidenceScore { get; init; }

    public int StartingProgress { get; init; }

    public int AgentCount { get; init; }

    public int CompanyCount { get; init; }

    public int ContactCount { get; init; }

    public int DossierCount { get; init; }

    public IReadOnlyList<DashboardAgentNodeViewModel> SwarmNodes { get; init; } = Array.Empty<DashboardAgentNodeViewModel>();

    public IReadOnlyList<DashboardLeadViewModel> TopLeads { get; init; } = Array.Empty<DashboardLeadViewModel>();

    public IReadOnlyList<DashboardSignalViewModel> RecentSignals { get; init; } = Array.Empty<DashboardSignalViewModel>();

    public IReadOnlyList<DashboardEntityLinkViewModel> EntityNavigation { get; init; } = Array.Empty<DashboardEntityLinkViewModel>();
}
