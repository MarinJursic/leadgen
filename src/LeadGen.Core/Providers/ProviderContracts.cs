using LeadGen.Core.Domain;

namespace LeadGen.Core.Providers;

public sealed record AiRequest(
    string Purpose,
    string SystemPrompt,
    string UserPrompt,
    decimal Temperature = 0.1m,
    int MaxOutputTokens = 1200);

public sealed record IcpProfile(
    string Summary,
    IReadOnlyList<string> TargetIndustries,
    IReadOnlyList<string> TargetLocations,
    IReadOnlyList<string> BuyerTypes,
    IReadOnlyList<string> PainPoints,
    IReadOnlyList<string> PositiveSignals,
    IReadOnlyList<string> NegativeSignals,
    IReadOnlyList<string> SearchKeywords,
    IReadOnlyList<string> ExampleQueries);

public sealed record SearchPlan(IReadOnlyList<SearchPlanQuery> Queries);

public sealed record SearchPlanQuery(string Query, string Purpose);

public sealed record SearchResultDto(
    string Title,
    string Url,
    string Snippet,
    string Domain);

public sealed record ExtractedPageDto(
    string Url,
    string Title,
    string Text,
    string Domain);

public sealed record ContactCandidate(
    LeadContactType Type,
    string Value,
    string? SourceUrl,
    int ConfidenceScore,
    bool IsVerified);

public interface IAiClient
{
    Task<T> GenerateJsonAsync<T>(AiRequest request, CancellationToken ct);

    Task<string> GenerateTextAsync(AiRequest request, CancellationToken ct);
}

public interface IWebSearchClient
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int maxResults, CancellationToken ct);

    Task<IReadOnlyList<ExtractedPageDto>> ExtractAsync(IEnumerable<string> urls, CancellationToken ct);
}

public interface IContactEnrichmentClient
{
    Task<IReadOnlyList<ContactCandidate>> FindContactsAsync(string companyName, string? domain, string? websiteUrl, IEnumerable<ExtractedPageDto> pages, CancellationToken ct);
}
