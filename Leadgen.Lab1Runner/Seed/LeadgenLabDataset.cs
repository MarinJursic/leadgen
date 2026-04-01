using Leadgen.Model.Entities;

namespace Leadgen.Lab1Runner.Seed;

public class LeadgenLabDataset
{
    public List<BusinessDnaMission> Missions { get; init; } = new();

    public List<SwarmAgent> Agents { get; init; } = new();
}
