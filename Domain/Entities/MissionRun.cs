using Leadgen.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class MissionRun
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(60)]
    public string RunCode { get; set; } = string.Empty;

    [ForeignKey(nameof(Mission))]
    public Guid BusinessDnaMissionId { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public MissionStatus Status { get; set; }

    [Required]
    [MaxLength(160)]
    public string SearchRegion { get; set; } = string.Empty;

    public int TokenBudget { get; set; }

    public decimal EstimatedCostUsd { get; set; }

    public virtual BusinessDnaMission? Mission { get; set; }

    public virtual ICollection<MissionAgentAssignment> AgentAssignments { get; set; } = new List<MissionAgentAssignment>();

    public virtual ICollection<TargetCompany> TargetCompanies { get; set; } = new List<TargetCompany>();

    public virtual ICollection<LeadDossier> LeadDossiers { get; set; } = new List<LeadDossier>();
}
