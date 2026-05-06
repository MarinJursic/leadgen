using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class LeadDossier
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(MissionRun))]
    public Guid MissionRunId { get; set; }

    [ForeignKey(nameof(TargetCompany))]
    public Guid TargetCompanyId { get; set; }

    [ForeignKey(nameof(TargetContact))]
    public Guid TargetContactId { get; set; }

    public int LeadgenScore { get; set; }

    [Required]
    [MaxLength(600)]
    public string SuggestedApproach { get; set; } = string.Empty;

    [Required]
    [MaxLength(600)]
    public string AdvantagePoint { get; set; } = string.Empty;

    public bool IsReadyForOutreach { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastUpdatedAtUtc { get; set; }

    public int SupportingEvidenceCount { get; set; }

    public virtual MissionRun? MissionRun { get; set; }

    public virtual TargetCompany? TargetCompany { get; set; }

    public virtual TargetContact? TargetContact { get; set; }
}
