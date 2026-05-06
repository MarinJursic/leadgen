using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class TargetContact
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(TargetCompany))]
    public Guid TargetCompanyId { get; set; }

    [Required]
    [MaxLength(180)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Seniority { get; set; } = string.Empty;

    public bool IsDecisionMaker { get; set; }

    [MaxLength(320)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(120)]
    public string? XHandle { get; set; }

    [MaxLength(120)]
    public string? GitHubUsername { get; set; }

    [Required]
    [MaxLength(500)]
    public string OpportunitySummary { get; set; } = string.Empty;

    public DateTime LastObservedAtUtc { get; set; }

    public virtual TargetCompany? TargetCompany { get; set; }

    public virtual ICollection<ContactChannel> ContactChannels { get; set; } = new List<ContactChannel>();

    public virtual ICollection<EvidencePoint> EvidencePoints { get; set; } = new List<EvidencePoint>();

    public virtual ICollection<LeadDossier> LeadDossiers { get; set; } = new List<LeadDossier>();
}
