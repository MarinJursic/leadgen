using Leadgen.Model.Entities;

namespace leadgen.ViewModels.SwarmAgents;

public sealed class SwarmAgentDetailsViewModel
{
    public required SwarmAgent Agent { get; init; }

    public IReadOnlyList<MissionAgentAssignment> Assignments { get; init; } = Array.Empty<MissionAgentAssignment>();
}
