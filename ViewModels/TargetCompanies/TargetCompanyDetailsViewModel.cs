using Leadgen.Model.Entities;

namespace leadgen.ViewModels.TargetCompanies;

public sealed class TargetCompanyDetailsViewModel
{
    public required TargetCompany Company { get; init; }

    public BusinessDnaMission? Mission { get; init; }

    public MissionRun? Run { get; init; }

    public IReadOnlyList<LeadDossier> Dossiers { get; init; } = Array.Empty<LeadDossier>();
}
