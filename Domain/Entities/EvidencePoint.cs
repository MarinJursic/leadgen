using Leadgen.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class EvidencePoint
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(TargetContact))]
    public Guid TargetContactId { get; set; }

    public EvidenceKind Kind { get; set; }

    [Required]
    [MaxLength(160)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string SourcePlatform { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string SourceUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string RawSnippet { get; set; } = string.Empty;

    public DateTime CapturedAtUtc { get; set; }

    public decimal ConfidenceScore { get; set; }

    public bool IsQualificationSignal { get; set; }

    public virtual TargetContact? TargetContact { get; set; }
}
