using Leadgen.Model.Entities;

namespace leadgen.ViewModels.LeadDossiers;

public sealed class LeadDossierDetailsViewModel
{
    public required LeadDossier Dossier { get; init; }

    public MissionRun? Run { get; init; }

    public BusinessDnaMission? Mission { get; init; }

    public TargetCompany? Company { get; init; }

    public TargetContact? Contact { get; init; }
}
