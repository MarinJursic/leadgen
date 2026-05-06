using Leadgen.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Leadgen.Model.Entities;

public class SwarmAgent
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(60)]
    public string CodeName { get; set; } = string.Empty;

    public AgentRole Role { get; set; }

    [Required]
    [MaxLength(80)]
    public string Provider { get; set; } = string.Empty;

    public decimal Temperature { get; set; }

    public int MaxConcurrentTasks { get; set; }

    public bool IsActive { get; set; }

    public DateTime LastHeartbeatUtc { get; set; }

    [Required]
    [MaxLength(320)]
    public string CurrentFocus { get; set; } = string.Empty;

    public virtual ICollection<MissionAgentAssignment> MissionAssignments { get; set; } = new List<MissionAgentAssignment>();
}
