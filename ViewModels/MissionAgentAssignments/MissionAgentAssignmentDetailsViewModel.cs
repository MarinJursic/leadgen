using Leadgen.Model.Entities;

namespace leadgen.ViewModels.MissionAgentAssignments;

public sealed class MissionAgentAssignmentDetailsViewModel
{
    public required MissionAgentAssignment Assignment { get; init; }

    public MissionRun? Run { get; init; }

    public BusinessDnaMission? Mission { get; init; }

    public SwarmAgent? Agent { get; init; }
}
