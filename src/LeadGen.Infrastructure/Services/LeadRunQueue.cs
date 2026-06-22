using System.Text.Json;
using System.Threading.Channels;
using LeadGen.Core.Configuration;
using LeadGen.Core.Domain;
using LeadGen.Core.Services;
using LeadGen.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeadGen.Infrastructure.Services;

internal interface ILeadRunQueueReader
{
    ValueTask<Guid> DequeueAsync(CancellationToken ct);
}

internal sealed class LeadRunQueue : ILeadRunQueue, ILeadRunQueueReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LeadGenOptions _options;

    public LeadRunQueue(IServiceScopeFactory scopeFactory, IOptions<LeadGenOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public async Task<Guid> EnqueueAsync(Guid campaignId, int requestedLeadCount, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LeadGenDbContext>();
        var campaign = await db.Campaigns
            .AsNoTracking()
            .Where(item => item.Id == campaignId)
            .Select(item => new { item.Id, item.Name })
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Campaign was not found.");

        var boundedCount = Math.Clamp(requestedLeadCount <= 0 ? 5 : requestedLeadCount, 1, Math.Max(1, _options.MaxLeadsPerRun));
        var run = new LeadSearchRun
        {
            CampaignId = campaign.Id,
            RequestedLeadCount = boundedCount,
            Status = LeadSearchRunStatus.Queued,
            LogsJson = JsonSerializer.Serialize(new[]
            {
                "Queued by web request",
                GraphEvent("Base", campaign.Name)
            }, JsonOptions)
        };

        db.LeadSearchRuns.Add(run);
        await db.SaveChangesAsync(ct);
        await _queue.Writer.WriteAsync(run.Id, ct);
        return run.Id;
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken ct)
    {
        return _queue.Reader.ReadAsync(ct);
    }

    private static string GraphEvent(string type, params string?[] values)
    {
        return "Graph|" + type + "|" + string.Join('|', values.Select(SafeGraphValue));
    }

    private static string SafeGraphValue(string? value)
    {
        return (value ?? string.Empty)
            .Replace("\r", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("\n", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("|", "/", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }
}

internal sealed class LeadRunWorker : BackgroundService
{
    private readonly ILeadRunQueueReader _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LeadRunWorker> _logger;

    public LeadRunWorker(
        ILeadRunQueueReader queue,
        IServiceScopeFactory scopeFactory,
        ILogger<LeadRunWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid runId;
            try
            {
                runId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var workflow = scope.ServiceProvider.GetRequiredService<ILeadDiscoveryWorkflow>();
                await workflow.ExecuteRunAsync(runId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Queued lead run {RunId} failed outside workflow handling", runId);
            }
        }
    }
}
