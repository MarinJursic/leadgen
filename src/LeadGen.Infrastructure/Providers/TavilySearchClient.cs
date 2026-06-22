using System.Net.Http.Json;
using System.Text.Json;
using LeadGen.Core.Configuration;
using LeadGen.Core.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeadGen.Infrastructure.Providers;

public sealed class TavilySearchClient : IWebSearchClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] ExcludedDomains =
    [
        "linkedin.com", "facebook.com", "instagram.com", "youtube.com", "x.com", "twitter.com",
        "medium.com", "substack.com", "wordpress.com", "blogspot.com", "blogger.com",
        "yelp.com", "tripadvisor.com", "clutch.co", "goodfirms.co", "designrush.com",
        "upcity.com", "g2.com", "capterra.com", "softwareadvice.com", "trade.gov",
        "europa.eu", "wikipedia.org", "wikidata.org", "state.gov"
    ];

    private readonly HttpClient _httpClient;
    private readonly LeadGenOptions _options;
    private readonly ILogger<TavilySearchClient> _logger;

    public TavilySearchClient(IOptions<LeadGenOptions> options, ILogger<TavilySearchClient> logger)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.tavily.com/"),
            Timeout = TimeSpan.FromSeconds(Math.Clamp(_options.ProviderTimeoutSeconds, 5, 120))
        };
    }

    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int maxResults, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.TavilyApiKey))
        {
            throw new InvalidOperationException("Tavily API key is not configured.");
        }

        var boundedMax = Math.Clamp(maxResults, 1, Math.Clamp(_options.MaxSearchResultsPerQuery, 1, 20));
        var request = new Dictionary<string, object?>
        {
            ["api_key"] = _options.TavilyApiKey,
            ["query"] = query,
            ["max_results"] = boundedMax,
            ["search_depth"] = "basic",
            ["topic"] = "general",
            ["exclude_domains"] = ExcludedDomains,
            ["include_answer"] = false,
            ["include_raw_content"] = false
        };

        var country = DetectCountry(query);
        if (!string.IsNullOrWhiteSpace(country))
        {
            request["country"] = country;
        }

        using var response = await _httpClient.PostAsJsonAsync("search", request, JsonOptions, ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Tavily search failed with HTTP {StatusCode}", (int)response.StatusCode);
            throw new InvalidOperationException($"Tavily search failed with HTTP {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("results", out var results))
        {
            return [];
        }

        return results.EnumerateArray()
            .Take(boundedMax)
            .Select(item =>
            {
                var url = item.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? "" : "";
                return new SearchResultDto(
                    item.TryGetProperty("title", out var title) ? title.GetString() ?? url : url,
                    url,
                    item.TryGetProperty("content", out var content) ? content.GetString() ?? "" : "",
                    NormalizeDomain(url));
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Url))
            .ToList();
    }

    public async Task<IReadOnlyList<ExtractedPageDto>> ExtractAsync(IEnumerable<string> urls, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.TavilyApiKey))
        {
            throw new InvalidOperationException("Tavily API key is not configured.");
        }

        var boundedUrls = urls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, _options.MaxExtractUrlsPerRun))
            .ToList();

        if (boundedUrls.Count == 0)
        {
            return [];
        }

        var pages = new List<ExtractedPageDto>();
        foreach (var batch in boundedUrls.Chunk(20))
        {
            pages.AddRange(await ExtractBatchAsync(batch, ct));
        }

        return pages;
    }

    private async Task<IReadOnlyList<ExtractedPageDto>> ExtractBatchAsync(IReadOnlyList<string> urls, CancellationToken ct)
    {
        using var response = await _httpClient.PostAsJsonAsync("extract", new
        {
            api_key = _options.TavilyApiKey,
            urls,
            query = "email contact kontakt phone address outreach",
            chunks_per_source = 5,
            extract_depth = "basic",
            format = "text",
            timeout = Math.Clamp(_options.ProviderTimeoutSeconds, 5, 60)
        }, JsonOptions, ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Tavily extract failed with HTTP {StatusCode}", (int)response.StatusCode);
            throw new InvalidOperationException($"Tavily extract failed with HTTP {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("results", out var results))
        {
            return [];
        }

        return results.EnumerateArray()
            .Select(item =>
            {
                var url = item.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? "" : "";
                var text = item.TryGetProperty("raw_content", out var raw) ? raw.GetString() ?? "" : "";
                if (string.IsNullOrWhiteSpace(text) && item.TryGetProperty("content", out var content))
                {
                    text = content.GetString() ?? "";
                }

                return new ExtractedPageDto(url, url, Clip(text, 4000), NormalizeDomain(url));
            })
            .Where(page => !string.IsNullOrWhiteSpace(page.Url))
            .ToList();
    }

    private static string NormalizeDomain(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url.Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .Split('/')[0]
                .ToLowerInvariant();
        }

        return uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
            ? uri.Host[4..].ToLowerInvariant()
            : uri.Host.ToLowerInvariant();
    }

    private static string Clip(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    private static string? DetectCountry(string query)
    {
        return query.Contains("croatia", StringComparison.OrdinalIgnoreCase)
            || query.Contains("hrvatska", StringComparison.OrdinalIgnoreCase)
            || query.Contains("zagreb", StringComparison.OrdinalIgnoreCase)
            ? "croatia"
            : null;
    }
}
