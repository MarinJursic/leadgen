using Leadgen.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Leadgen.Model.Entities;

public class BusinessDnaMission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string MissionName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Mechanic { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string PrimarySurface { get; set; } = string.Empty;

    public List<string> SurfaceTags { get; set; } = new();

    [Required]
    [MaxLength(240)]
    public string Persona { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Villain { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Delta { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public MissionStatus Status { get; set; }

    public virtual ICollection<ClarificationQuestion> ClarificationQuestions { get; set; } = new List<ClarificationQuestion>();

    public virtual ICollection<MissionRun> Runs { get; set; } = new List<MissionRun>();
}
