using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace leadgen.Data;

public sealed class LeadgenDbContextFactory : IDesignTimeDbContextFactory<LeadgenDbContext>
{
    public LeadgenDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("LeadgenDb") ?? "Data Source=leadgen-lab3.db";
        var options = new DbContextOptionsBuilder<LeadgenDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new LeadgenDbContext(options);
    }
}
