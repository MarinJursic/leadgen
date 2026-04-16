using Leadgen.Model.Entities;

namespace leadgen.ViewModels.EvidencePoints;

public sealed class EvidencePointDetailsViewModel
{
    public required EvidencePoint Evidence { get; init; }

    public TargetContact? Contact { get; init; }

    public TargetCompany? Company { get; init; }

    public BusinessDnaMission? Mission { get; init; }
}
