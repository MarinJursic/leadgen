using Leadgen.Model.Entities;

namespace leadgen.ViewModels.TargetContacts;

public sealed class TargetContactDetailsViewModel
{
    public required TargetContact Contact { get; init; }

    public TargetCompany? Company { get; init; }

    public BusinessDnaMission? Mission { get; init; }

    public MissionRun? Run { get; init; }

    public IReadOnlyList<LeadDossier> Dossiers { get; init; } = Array.Empty<LeadDossier>();
}
