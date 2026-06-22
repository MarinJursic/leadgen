using LeadGen.Core.Domain;
using LeadGen.Core.Providers;

namespace LeadGen.Core.Services;

public sealed record RunSummary(
    Guid RunId,
    LeadSearchRunStatus Status,
    int LeadCount,
    string? ErrorMessage);

public sealed record GlobalSearchResult(
    string Type,
    string Title,
    string Subtitle,
    string Url);

public interface ILeadDiscoveryWorkflow
{
    Task<IcpProfile> GenerateIcpAsync(Guid campaignId, CancellationToken ct);

    Task<RunSummary> StartRunAsync(Guid campaignId, int requestedLeadCount, CancellationToken ct);

    Task<RunSummary> ExecuteRunAsync(Guid runId, CancellationToken ct);
}

public interface ILeadRunQueue
{
    Task<Guid> EnqueueAsync(Guid campaignId, int requestedLeadCount, CancellationToken ct);
}

public interface IGlobalSearchService
{
    Task<IReadOnlyList<GlobalSearchResult>> SearchAsync(string? query, int take, CancellationToken ct);
}

public interface IAppLogReader
{
    Task<IReadOnlyList<string>> TailAsync(int take, CancellationToken ct);
}

public interface IAppLogWriter
{
    Task WriteAsync(string level, string category, string message, string? correlationId, CancellationToken ct);
}
