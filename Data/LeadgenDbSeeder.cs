using Leadgen.Lab1Runner.Seed;

namespace leadgen.Data;

public static class LeadgenDbSeeder
{
    public static async Task SeedAsync(LeadgenDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (dbContext.BusinessDnaMissions.Any() || dbContext.SwarmAgents.Any())
        {
            return;
        }

        var dataset = LeadgenSeedFactory.Create();

        dbContext.SwarmAgents.AddRange(dataset.Agents);
        dbContext.BusinessDnaMissions.AddRange(dataset.Missions);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
