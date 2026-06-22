using System.Text.Json;
using System.Text.Json.Serialization;
using LeadGen.Core.Domain;
using LeadGen.Core.Services;
using LeadGen.Infrastructure;
using LeadGen.Infrastructure.Data;
using LeadGen.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = false
};
jsonOptions.Converters.Add(new JsonStringEnumConverter());

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("src/LeadGen.Web/appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
Directory.CreateDirectory(appDataPath);

var services = new ServiceCollection();
var logStore = new SafeFileLogStore(Path.Combine(appDataPath, "logs"));
services.AddSingleton<IAppLogReader>(logStore);
services.AddSingleton<IAppLogWriter>(logStore);
services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddProvider(logStore);
});
services.AddLeadGenInfrastructure(configuration);

await using var provider = services.BuildServiceProvider();
await MigrateDatabaseAsync(provider, Path.Combine(appDataPath, "leadgen.migration.lock"));

if (args.Length > 0)
{
    var tool = args[0] == "--tool" && args.Length > 1 ? args[1] : args[0];
    var arguments = ParseArgs(args.Skip(args[0] == "--tool" ? 2 : 1));
    var result = await RunToolAsync(provider, tool, arguments);
    Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
    return;
}

Console.Error.WriteLine("LeadGen MCP stdio server ready.");
string? line;
while ((line = await Console.In.ReadLineAsync()) is not null)
{
    if (string.IsNullOrWhiteSpace(line))
    {
        continue;
    }

    try
    {
        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        var id = root.TryGetProperty("id", out var idElement) ? idElement.Clone() : default;
        var tool = root.TryGetProperty("tool", out var toolElement)
            ? toolElement.GetString()
            : root.TryGetProperty("method", out var methodElement)
                ? methodElement.GetString()
                : null;
        var arguments = root.TryGetProperty("arguments", out var argsElement)
            ? ToDictionary(argsElement)
            : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(tool))
        {
            throw new InvalidOperationException("Tool name is required.");
        }

        var result = await RunToolAsync(provider, tool, arguments);
        Console.WriteLine(JsonSerializer.Serialize(new { id, result }, jsonOptions));
    }
    catch (Exception ex)
    {
        Console.WriteLine(JsonSerializer.Serialize(new { error = new { message = ex.Message } }, jsonOptions));
    }
}

static Dictionary<string, string?> ParseArgs(IEnumerable<string> args)
{
    var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    string? pendingKey = null;
    foreach (var arg in args)
    {
        if (arg.StartsWith("--", StringComparison.Ordinal))
        {
            pendingKey = arg[2..];
            values[pendingKey] = "true";
            continue;
        }

        if (pendingKey is not null)
        {
            values[pendingKey] = arg;
            pendingKey = null;
        }
    }

    return values;
}

static Dictionary<string, string?> ToDictionary(JsonElement element)
{
    var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    foreach (var property in element.EnumerateObject())
    {
        values[property.Name] = property.Value.ValueKind == JsonValueKind.String
            ? property.Value.GetString()
            : property.Value.GetRawText();
    }

    return values;
}

static async Task MigrateDatabaseAsync(IServiceProvider provider, string lockPath)
{
    await using var migrationLock = await AcquireMigrationLockAsync(lockPath);
    using var scope = provider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LeadGenDbContext>();
    await db.Database.MigrateAsync();
}

static async Task<FileStream> AcquireMigrationLockAsync(string path)
{
    for (var attempt = 0; attempt < 100; attempt++)
    {
        try
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException) when (attempt < 99)
        {
            await Task.Delay(100);
        }
    }

    throw new IOException("Could not acquire the LeadGen migration lock.");
}

static async Task<object> RunToolAsync(IServiceProvider provider, string tool, IReadOnlyDictionary<string, string?> args)
{
    using var scope = provider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LeadGenDbContext>();
    var workflow = scope.ServiceProvider.GetRequiredService<ILeadDiscoveryWorkflow>();

    return tool switch
    {
        "leadgen_health" => new { status = "ok" },
        "list_campaigns" => await ListCampaignsAsync(db, args.GetValueOrDefault("search")),
        "get_campaign" => await GetCampaignAsync(db, RequiredGuid(args, "campaignId")),
        "create_campaign" => await CreateCampaignAsync(db, args),
        "start_lead_run" => await workflow.StartRunAsync(RequiredGuid(args, "campaignId"), ReadInt(args, "leadCount", 5), CancellationToken.None),
        "get_run" => await GetRunAsync(db, RequiredGuid(args, "runId")),
        "search_leads" => await SearchLeadsAsync(db, args.GetValueOrDefault("query") ?? ""),
        "get_lead_dossier" => await GetLeadDossierAsync(db, RequiredGuid(args, "leadId")),
        "update_lead_status" => await UpdateLeadStatusAsync(db, RequiredGuid(args, "leadId"), args.GetValueOrDefault("status") ?? "New"),
        "add_lead_note" => await AddLeadNoteAsync(db, RequiredGuid(args, "leadId"), args.GetValueOrDefault("body") ?? ""),
        _ => throw new InvalidOperationException($"Unknown tool '{tool}'.")
    };
}

static async Task<object> ListCampaignsAsync(LeadGenDbContext db, string? search)
{
    var query = db.Campaigns.AsNoTracking().Include(campaign => campaign.Leads).AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(campaign => campaign.Name.Contains(search) || campaign.BusinessName.Contains(search));
    }

    return await query.OrderByDescending(campaign => campaign.UpdatedAtUtc)
        .Take(25)
        .Select(campaign => new
        {
            campaign.Id,
            campaign.Name,
            campaign.BusinessName,
            leadCount = campaign.Leads.Count
        })
        .ToListAsync();
}

static async Task<object> GetCampaignAsync(LeadGenDbContext db, Guid id)
{
    return await db.Campaigns.AsNoTracking()
        .Include(campaign => campaign.Runs)
        .Include(campaign => campaign.Leads)
        .Where(campaign => campaign.Id == id)
        .Select(campaign => new
        {
            campaign.Id,
            campaign.Name,
            campaign.BusinessName,
            campaign.BusinessDescription,
            campaign.IcpJson,
            runCount = campaign.Runs.Count,
            leadCount = campaign.Leads.Count
        })
        .FirstOrDefaultAsync()
        ?? throw new InvalidOperationException("Campaign was not found.");
}

static async Task<object> CreateCampaignAsync(LeadGenDbContext db, IReadOnlyDictionary<string, string?> args)
{
    var campaign = new Campaign
    {
        Name = Required(args, "name"),
        BusinessName = Required(args, "businessName"),
        BusinessDescription = Required(args, "businessDescription"),
        WebsiteUrl = args.GetValueOrDefault("websiteUrl"),
        TargetGeography = args.GetValueOrDefault("targetGeography"),
        TargetCustomers = args.GetValueOrDefault("targetCustomers"),
        Exclusions = args.GetValueOrDefault("exclusions"),
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };
    db.Campaigns.Add(campaign);
    await db.SaveChangesAsync();
    return new { campaign.Id, campaign.Name };
}

static async Task<object> GetRunAsync(LeadGenDbContext db, Guid id)
{
    return await db.LeadSearchRuns.AsNoTracking()
        .Include(run => run.Leads)
        .Where(run => run.Id == id)
        .Select(run => new
        {
            run.Id,
            run.CampaignId,
            run.Status,
            run.ErrorMessage,
            run.SearchQueriesJson,
            run.LogsJson,
            leadCount = run.Leads.Count
        })
        .FirstOrDefaultAsync()
        ?? throw new InvalidOperationException("Run was not found.");
}

static async Task<object> SearchLeadsAsync(LeadGenDbContext db, string query)
{
    return await db.Leads.AsNoTracking()
        .Where(lead => string.IsNullOrWhiteSpace(query)
            || lead.CompanyName.Contains(query)
            || (lead.Domain != null && lead.Domain.Contains(query))
            || lead.DossierMarkdown.Contains(query))
        .OrderByDescending(lead => lead.FitScore)
        .Take(25)
        .Select(lead => new
        {
            lead.Id,
            lead.CompanyName,
            lead.Domain,
            lead.FitScore,
            lead.Status
        })
        .ToListAsync();
}

static async Task<object> GetLeadDossierAsync(LeadGenDbContext db, Guid id)
{
    return await db.Leads.AsNoTracking()
        .Include(lead => lead.Contacts)
        .Where(lead => lead.Id == id)
        .Select(lead => new
        {
            lead.Id,
            lead.CompanyName,
            lead.Domain,
            lead.FitScore,
            lead.MatchReasonsJson,
            lead.EvidenceJson,
            lead.DossierMarkdown,
            lead.SuggestedOutreachAngle,
            contacts = lead.Contacts.Select(contact => new
            {
                contact.Type,
                contact.Value,
                contact.SourceUrl,
                contact.ConfidenceScore
            })
        })
        .FirstOrDefaultAsync()
        ?? throw new InvalidOperationException("Lead was not found.");
}

static async Task<object> UpdateLeadStatusAsync(LeadGenDbContext db, Guid id, string statusValue)
{
    var lead = await db.Leads.FirstOrDefaultAsync(item => item.Id == id)
        ?? throw new InvalidOperationException("Lead was not found.");
    if (!Enum.TryParse<LeadStatus>(statusValue, ignoreCase: true, out var status))
    {
        throw new InvalidOperationException("Invalid lead status.");
    }

    lead.Status = status;
    lead.UpdatedAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return new { lead.Id, lead.Status };
}

static async Task<object> AddLeadNoteAsync(LeadGenDbContext db, Guid leadId, string body)
{
    if (string.IsNullOrWhiteSpace(body))
    {
        throw new InvalidOperationException("Note body is required.");
    }

    if (!await db.Leads.AnyAsync(lead => lead.Id == leadId))
    {
        throw new InvalidOperationException("Lead was not found.");
    }

    var note = new LeadNote
    {
        LeadId = leadId,
        Body = body.Trim(),
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };
    db.LeadNotes.Add(note);
    await db.SaveChangesAsync();
    return new { note.Id, note.LeadId };
}

static Guid RequiredGuid(IReadOnlyDictionary<string, string?> args, string key)
{
    var value = Required(args, key);
    return Guid.TryParse(value, out var id) ? id : throw new InvalidOperationException($"{key} must be a GUID.");
}

static string Required(IReadOnlyDictionary<string, string?> args, string key)
{
    return args.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new InvalidOperationException($"{key} is required.");
}

static int ReadInt(IReadOnlyDictionary<string, string?> args, string key, int fallback)
{
    return args.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : fallback;
}
