namespace Leadgen.Model.Entities;

public class LeadDossier
{
    public Guid Id { get; set; }

    public Guid MissionRunId { get; set; }

    public Guid TargetCompanyId { get; set; }

    public Guid TargetContactId { get; set; }

    public int LeadgenScore { get; set; }

    public string SuggestedApproach { get; set; } = string.Empty;

    public string AdvantagePoint { get; set; } = string.Empty;

    public bool IsReadyForOutreach { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }

    public int SupportingEvidenceCount { get; set; }
}
