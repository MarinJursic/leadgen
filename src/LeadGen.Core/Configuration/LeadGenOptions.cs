namespace LeadGen.Core.Configuration;

public sealed class LeadGenOptions
{
    public const string SectionName = "LeadGen";

    public bool EnableAdminLogViewer { get; set; } = true;

    public int MaxSearchQueriesPerRun { get; set; } = 5;

    public int MaxSearchResultsPerQuery { get; set; } = 5;

    public int MaxExtractUrlsPerRun { get; set; } = 20;

    public int MaxLeadsPerRun { get; set; } = 10;

    public int MaxConcurrentProviderCalls { get; set; } = 2;

    public decimal MaxRunEstimatedCostUsd { get; set; } = 0.25m;

    public int ProviderTimeoutSeconds { get; set; } = 25;

    public string DeepSeekBaseUrl { get; set; } = "https://api.deepseek.com";

    public string DeepSeekModel { get; set; } = "deepseek-v4-flash";

    public string? DeepSeekApiKey { get; set; }

    public string? TavilyApiKey { get; set; }
}
