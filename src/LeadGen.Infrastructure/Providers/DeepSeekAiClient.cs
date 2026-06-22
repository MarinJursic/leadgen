using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LeadGen.Core.Configuration;
using LeadGen.Core.Domain;
using LeadGen.Core.Providers;
using LeadGen.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeadGen.Infrastructure.Providers;

public sealed class DeepSeekAiClient : IAiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly LeadGenOptions _options;
    private readonly LeadGenDbContext _db;
    private readonly ILogger<DeepSeekAiClient> _logger;

    public DeepSeekAiClient(IOptions<LeadGenOptions> options, LeadGenDbContext db, ILogger<DeepSeekAiClient> logger)
    {
        _options = options.Value;
        _db = db;
        _logger = logger;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.DeepSeekBaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(Math.Clamp(_options.ProviderTimeoutSeconds, 5, 120))
        };
    }

    public async Task<T> GenerateJsonAsync<T>(AiRequest request, CancellationToken ct)
    {
        var schema = SchemaFor<T>();
        var strictRequest = request with
        {
            SystemPrompt = BuildJsonSystemPrompt(request.SystemPrompt, schema),
            MaxOutputTokens = Math.Max(request.MaxOutputTokens, MinimumJsonTokens<T>())
        };

        var text = await CompleteAsync(strictRequest, jsonMode: true, ct);
        try
        {
            return DeserializePayload<T>(text);
        }
        catch (JsonException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "DeepSeek returned invalid JSON for purpose {Purpose}; retrying JSON repair", request.Purpose);
            var repaired = await CompleteAsync(new AiRequest(
                $"{request.Purpose}JsonRepair",
                BuildJsonRepairSystemPrompt(schema),
                $"Original purpose: {request.Purpose}\nMalformed JSON payload:\n{ClipForPrompt(text, 6000)}",
                Temperature: 0,
                MaxOutputTokens: Math.Max(request.MaxOutputTokens, MinimumJsonTokens<T>())), jsonMode: true, ct);

            try
            {
                return DeserializePayload<T>(repaired);
            }
            catch (JsonException repairEx)
            {
                throw new InvalidOperationException($"DeepSeek returned invalid JSON for {request.Purpose} after a repair attempt.", repairEx);
            }
        }
    }

    public Task<string> GenerateTextAsync(AiRequest request, CancellationToken ct)
    {
        return CompleteAsync(request, jsonMode: false, ct);
    }

    private async Task<string> CompleteAsync(AiRequest request, bool jsonMode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.DeepSeekApiKey))
        {
            throw new InvalidOperationException("DeepSeek API key is not configured.");
        }

        var stopwatch = Stopwatch.StartNew();
        var success = false;
        string? safeError = null;
        int? inputTokens = null;
        int? outputTokens = null;

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.DeepSeekApiKey);
            message.Content = JsonContent.Create(new
            {
                model = _options.DeepSeekModel,
                temperature = request.Temperature,
                max_tokens = request.MaxOutputTokens,
                response_format = jsonMode ? new { type = "json_object" } : null,
                messages = new object[]
                {
                    new { role = "system", content = request.SystemPrompt },
                    new { role = "user", content = request.UserPrompt }
                }
            }, options: JsonOptions);

            using var response = await _httpClient.SendAsync(message, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"DeepSeek request failed with HTTP {(int)response.StatusCode}.");
            }

            using var document = JsonDocument.Parse(body);
            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (document.RootElement.TryGetProperty("usage", out var usage))
            {
                inputTokens = usage.TryGetProperty("prompt_tokens", out var promptTokens) ? promptTokens.GetInt32() : null;
                outputTokens = usage.TryGetProperty("completion_tokens", out var completionTokens) ? completionTokens.GetInt32() : null;
            }

            success = true;
            return content ?? throw new InvalidOperationException("DeepSeek returned no content.");
        }
        catch (Exception ex)
        {
            safeError = Safe(ex.Message);
            _logger.LogWarning(ex, "DeepSeek AI call failed for purpose {Purpose}", request.Purpose);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _db.AiCallLogs.Add(new AiCallLog
            {
                Purpose = request.Purpose,
                Provider = "DeepSeek",
                Model = _options.DeepSeekModel,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                EstimatedCostUsd = EstimateCost(inputTokens, outputTokens),
                DurationMs = Math.Max(1, (int)stopwatch.ElapsedMilliseconds),
                Success = success,
                ErrorMessage = safeError
            });
            await _db.SaveChangesAsync(CancellationToken.None);
        }
    }

    private static decimal EstimateCost(int? inputTokens, int? outputTokens)
    {
        var input = inputTokens.GetValueOrDefault() / 1_000_000m * 0.28m;
        var output = outputTokens.GetValueOrDefault() / 1_000_000m * 0.42m;
        return Math.Round(input + output, 6);
    }

    internal static T DeserializePayload<T>(string text)
    {
        var payload = ExtractJsonPayload(text);
        if (typeof(T) == typeof(SearchPlan))
        {
            return (T)(object)ParseSearchPlan(payload);
        }

        return JsonSerializer.Deserialize<T>(payload, JsonOptions)
            ?? throw new JsonException("DeepSeek returned empty JSON.");
    }

    internal static string ExtractJsonPayload(string text)
    {
        var trimmed = text.Trim();
        var start = -1;
        for (var i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] is '{' or '[')
            {
                start = i;
                break;
            }
        }

        if (start < 0)
        {
            return trimmed;
        }

        var expectedClosers = new Stack<char>();
        var inString = false;
        var escaped = false;

        for (var i = start; i < trimmed.Length; i++)
        {
            var value = trimmed[i];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (value == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (value == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (value == '"')
            {
                inString = true;
                continue;
            }

            if (value == '{')
            {
                expectedClosers.Push('}');
                continue;
            }

            if (value == '[')
            {
                expectedClosers.Push(']');
                continue;
            }

            if (value is not ('}' or ']'))
            {
                continue;
            }

            if (expectedClosers.Count == 0 || expectedClosers.Pop() != value)
            {
                return trimmed[start..].Trim();
            }

            if (expectedClosers.Count == 0)
            {
                return trimmed[start..(i + 1)].Trim();
            }
        }

        return trimmed[start..].Trim();
    }

    private static SearchPlan ParseSearchPlan(string text)
    {
        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;

        var queriesElement = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("queries", out var queries)
            ? queries
            : root;

        if (queriesElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("DeepSeek returned query JSON without a queries array.");
        }

        var items = new List<SearchPlanQuery>();
        foreach (var item in queriesElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var query = item.GetString();
                if (!string.IsNullOrWhiteSpace(query))
                {
                    items.Add(new SearchPlanQuery(query.Trim(), "Public web lead search."));
                }

                continue;
            }

            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var objectQuery = GetString(item, "query", "searchQuery", "q");
            if (string.IsNullOrWhiteSpace(objectQuery))
            {
                continue;
            }

            var purpose = GetString(item, "purpose", "reason", "rationale");
            items.Add(new SearchPlanQuery(
                objectQuery.Trim(),
                string.IsNullOrWhiteSpace(purpose) ? "Public web lead search." : purpose.Trim()));
        }

        return new SearchPlan(items);
    }

    private static string BuildJsonSystemPrompt(string basePrompt, string schema)
    {
        return $"""
        {basePrompt}

        Return only one valid compact JSON object. Do not include markdown, code fences, comments, prose, or trailing text.
        Use exactly the camelCase keys shown in the schema. Close every object and array.
        Do not use a "segments" wrapper or any alternate schema.
        Required schema:
        {schema}
        """;
    }

    private static string BuildJsonRepairSystemPrompt(string schema)
    {
        return $"""
        Repair the user's malformed JSON into one valid compact JSON object matching the required schema.
        Return only JSON. Do not include markdown, comments, explanation, or trailing text.
        Use exactly the camelCase keys shown in the schema. Close every object and array.
        Required schema:
        {schema}
        """;
    }

    private static string SchemaFor<T>()
    {
        if (typeof(T) == typeof(IcpProfile))
        {
            return """
            {
              "summary": "detailed ideal customer profile summary",
              "targetIndustries": ["specific industry or sub-industry"],
              "targetLocations": ["market, city, region, or service area"],
              "buyerTypes": ["specific buyer category with role/use case and why they would value the offer"],
              "painPoints": ["specific pain, operational friction, missed revenue, or buying trigger"],
              "positiveSignals": ["publicly observable buying signal or website/profile evidence"],
              "negativeSignals": ["disqualifying signal or wrong-fit pattern"],
              "searchKeywords": ["short search keyword phrase for this buyer category"],
              "exampleQueries": ["public web search query likely to find exact buyer organizations with contact routes"]
            }
            """;
        }

        if (typeof(T) == typeof(SearchPlan))
        {
            return """
            {
              "queries": [
                {
                  "query": "public web search query",
                  "purpose": "why this query should find matching companies"
                }
              ]
            }
            """;
        }

        return "{}";
    }

    private static int MinimumJsonTokens<T>()
    {
        if (typeof(T) == typeof(IcpProfile))
        {
            return 3000;
        }

        if (typeof(T) == typeof(SearchPlan))
        {
            return 1000;
        }

        return 1200;
    }

    private static string ClipForPrompt(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
        }

        return null;
    }

    private static string Safe(string value)
    {
        return value.Replace("\r", " ").Replace("\n", " ")[..Math.Min(value.Length, 500)];
    }
}
