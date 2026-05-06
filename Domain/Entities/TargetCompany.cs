using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class TargetCompany
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(MissionRun))]
    public Guid MissionRunId { get; set; }

    [Required]
    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string Domain { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string Industry { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string HeadquartersCity { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string HeadquartersCountry { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? OrganizationStageLabel { get; set; }

    public DateTime? LastSignalAtUtc { get; set; }

    public int EmployeeCount { get; set; }

    public bool IsHeadquartersVerified { get; set; }

    public decimal MatchScore { get; set; }

    public virtual MissionRun? MissionRun { get; set; }

    public virtual ICollection<TargetContact> Contacts { get; set; } = new List<TargetContact>();

    public virtual ICollection<LeadDossier> LeadDossiers { get; set; } = new List<LeadDossier>();
}
