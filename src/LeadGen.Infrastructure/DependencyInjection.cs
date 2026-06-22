using LeadGen.Core.Configuration;
using LeadGen.Core.Providers;
using LeadGen.Core.Services;
using LeadGen.Infrastructure.Data;
using LeadGen.Infrastructure.Providers;
using LeadGen.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LeadGen.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLeadGenInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LeadGenOptions>(options =>
        {
            configuration.GetSection(LeadGenOptions.SectionName).Bind(options);
            options.EnableAdminLogViewer = ReadBool(configuration, "ENABLE_ADMIN_LOG_VIEWER", options.EnableAdminLogViewer);
            options.DeepSeekApiKey = ReadString(configuration, "DEEPSEEK_API_KEY", options.DeepSeekApiKey);
            options.DeepSeekBaseUrl = ReadString(configuration, "DEEPSEEK_BASE_URL", options.DeepSeekBaseUrl) ?? options.DeepSeekBaseUrl;
            options.DeepSeekModel = ReadString(configuration, "DEEPSEEK_MODEL", options.DeepSeekModel) ?? options.DeepSeekModel;
            options.TavilyApiKey = ReadString(configuration, "TAVILY_API_KEY", options.TavilyApiKey);
            options.MaxSearchQueriesPerRun = ReadInt(configuration, "MAX_SEARCH_QUERIES_PER_RUN", options.MaxSearchQueriesPerRun);
            options.MaxSearchResultsPerQuery = ReadInt(configuration, "MAX_SEARCH_RESULTS_PER_QUERY", options.MaxSearchResultsPerQuery);
            options.MaxExtractUrlsPerRun = ReadInt(configuration, "MAX_EXTRACT_URLS_PER_RUN", options.MaxExtractUrlsPerRun);
            options.MaxLeadsPerRun = ReadInt(configuration, "MAX_LEADS_PER_RUN", options.MaxLeadsPerRun);
            options.ProviderTimeoutSeconds = ReadInt(configuration, "PROVIDER_TIMEOUT_SECONDS", options.ProviderTimeoutSeconds);
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=App_Data/leadgen.db";
        services.AddDbContext<LeadGenDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IAiClient, DeepSeekAiClient>();
        services.AddScoped<IWebSearchClient, TavilySearchClient>();
        services.AddScoped<IContactEnrichmentClient, PublicContactEnrichmentClient>();

        services.AddScoped<ILeadDiscoveryWorkflow, LeadDiscoveryWorkflow>();
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();
        services.AddSingleton<LeadRunQueue>();
        services.AddSingleton<ILeadRunQueue>(provider => provider.GetRequiredService<LeadRunQueue>());
        services.AddSingleton<ILeadRunQueueReader>(provider => provider.GetRequiredService<LeadRunQueue>());
        services.AddHostedService<LeadRunWorker>();
        return services;
    }

    private static string? ReadString(IConfiguration configuration, string key, string? fallback)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static bool ReadBool(IConfiguration configuration, string key, bool fallback)
    {
        return bool.TryParse(configuration[key], out var value) ? value : fallback;
    }

    private static int ReadInt(IConfiguration configuration, string key, int fallback)
    {
        return int.TryParse(configuration[key], out var value) ? value : fallback;
    }
}
