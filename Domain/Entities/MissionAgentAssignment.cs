using Leadgen.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadgen.Model.Entities;

public class MissionAgentAssignment
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(MissionRun))]
    public Guid MissionRunId { get; set; }

    [ForeignKey(nameof(SwarmAgent))]
    public Guid SwarmAgentId { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    [Required]
    [MaxLength(500)]
    public string Responsibility { get; set; } = string.Empty;

    public int TokenBudget { get; set; }

    public MissionStatus Status { get; set; }

    public virtual MissionRun? MissionRun { get; set; }

    public virtual SwarmAgent? SwarmAgent { get; set; }
}
