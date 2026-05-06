namespace leadgen.ViewModels.OutreachQueue;

public sealed class OutreachQueueItemViewModel
{
    public Guid DossierId { get; init; }

    public string MissionName { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string ContactName { get; init; } = string.Empty;

    public int LeadgenScore { get; init; }

    public string AdvantagePoint { get; init; } = string.Empty;

    public string SuggestedApproach { get; init; } = string.Empty;

    public DateTime LastUpdatedAtUtc { get; init; }
}
