using Leadgen.Model.Entities;

namespace leadgen.ViewModels.Missions;

public sealed class MissionDetailsViewModel
{
    public required BusinessDnaMission Mission { get; init; }

    public MissionRun? LatestRun { get; init; }

    public int CompanyCount { get; init; }

    public int ContactCount { get; init; }

    public int DossierCount { get; init; }
}
