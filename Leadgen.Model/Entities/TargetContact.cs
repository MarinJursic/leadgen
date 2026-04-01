namespace Leadgen.Model.Entities;

public class TargetContact
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Seniority { get; set; } = string.Empty;

    public bool IsDecisionMaker { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? XHandle { get; set; }

    public string? GitHubUsername { get; set; }

    public string OpportunitySummary { get; set; } = string.Empty;

    public DateTime LastObservedAtUtc { get; set; }

    public List<ContactChannel> ContactChannels { get; set; } = new();

    public List<EvidencePoint> EvidencePoints { get; set; } = new();
}
