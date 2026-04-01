namespace Leadgen.Model.Entities;

public class TargetCompany
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Domain { get; set; } = string.Empty;

    public string Industry { get; set; } = string.Empty;

    public string HeadquartersCity { get; set; } = string.Empty;

    public string HeadquartersCountry { get; set; } = string.Empty;

    public string? OrganizationStageLabel { get; set; }

    public DateTime? LastSignalAtUtc { get; set; }

    public int EmployeeCount { get; set; }

    public bool IsHeadquartersVerified { get; set; }

    public decimal MatchScore { get; set; }

    public List<TargetContact> Contacts { get; set; } = new();
}
