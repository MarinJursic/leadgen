using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace LeadGen.Tests;

public sealed class LeadGenApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"leadgen-test-{Guid.NewGuid():N}.db");
    private readonly Dictionary<string, string?> _previousEnvironment = new(StringComparer.OrdinalIgnoreCase);

    public LeadGenApiTestFactory()
    {
        SetEnvironment("ConnectionStrings__DefaultConnection", $"Data Source={_databasePath}");
        SetEnvironment("LeadGen__MaxLeadsPerRun", "10");
        SetEnvironment("LeadGen__MaxSearchQueriesPerRun", "8");
        SetEnvironment("LeadGen__MaxSearchResultsPerQuery", "10");
        SetEnvironment("LeadGen__MaxExtractUrlsPerRun", "60");
        SetEnvironment("LeadGen__EnableAdminLogViewer", "true");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                ["LeadGen:MaxLeadsPerRun"] = "10",
                ["LeadGen:MaxSearchQueriesPerRun"] = "8",
                ["LeadGen:MaxSearchResultsPerQuery"] = "10",
                ["LeadGen:MaxExtractUrlsPerRun"] = "60",
                ["LeadGen:EnableAdminLogViewer"] = "true"
            });
        });
    }

    public HttpClient CreateLeadGenClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = true
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var (key, value) in _previousEnvironment)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = _databasePath + suffix;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private void SetEnvironment(string key, string value)
    {
        _previousEnvironment[key] = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }
}
