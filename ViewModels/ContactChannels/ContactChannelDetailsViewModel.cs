using Leadgen.Model.Entities;

namespace leadgen.ViewModels.ContactChannels;

public sealed class ContactChannelDetailsViewModel
{
    public required ContactChannel Channel { get; init; }

    public TargetContact? Contact { get; init; }

    public TargetCompany? Company { get; init; }
}
