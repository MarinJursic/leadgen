using Leadgen.Model.Enums;

namespace Leadgen.Model.Entities;

public class MissionAgentAssignment
{
    public Guid Id { get; set; }

    public Guid MissionRunId { get; set; }

    public Guid SwarmAgentId { get; set; }

    public DateTime AssignedAtUtc { get; set; }

    public string Responsibility { get; set; } = string.Empty;

    public int TokenBudget { get; set; }

    public MissionStatus Status { get; set; }
}
