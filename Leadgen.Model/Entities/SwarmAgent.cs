using Leadgen.Model.Enums;

namespace Leadgen.Model.Entities;

public class SwarmAgent
{
    public Guid Id { get; set; }

    public string CodeName { get; set; } = string.Empty;

    public AgentRole Role { get; set; }

    public string Provider { get; set; } = string.Empty;

    public decimal Temperature { get; set; }

    public int MaxConcurrentTasks { get; set; }

    public bool IsActive { get; set; }

    public DateTime LastHeartbeatUtc { get; set; }

    public string CurrentFocus { get; set; } = string.Empty;

    public List<MissionAgentAssignment> MissionAssignments { get; set; } = new();
}
