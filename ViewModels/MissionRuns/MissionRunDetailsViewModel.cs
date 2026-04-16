using Leadgen.Model.Entities;

namespace leadgen.ViewModels.MissionRuns;

public sealed class MissionRunDetailsViewModel
{
    public required MissionRun Run { get; init; }

    public BusinessDnaMission? Mission { get; init; }

    public IReadOnlyList<MissionAgentAssignment> Assignments { get; init; } = Array.Empty<MissionAgentAssignment>();

    public IReadOnlyList<TargetCompany> Companies { get; init; } = Array.Empty<TargetCompany>();

    public IReadOnlyList<LeadDossier> Dossiers { get; init; } = Array.Empty<LeadDossier>();
}
