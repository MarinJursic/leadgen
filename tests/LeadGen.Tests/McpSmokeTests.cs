using System.Diagnostics;
using System.Text.Json;
using LeadGen.Core.Domain;
using LeadGen.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeadGen.Tests;

public sealed class McpSmokeTests
{
    [Fact]
    public async Task McpCli_Tools_CanReadAndUpdateLeadData()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"leadgen-mcp-{Guid.NewGuid():N}.db");
        try
        {
            var campaignId = Guid.NewGuid();
            var leadId = Guid.NewGuid();
            await SeedDatabaseAsync(databasePath, campaignId, leadId);

            using var health = await RunMcpToolAsync(databasePath, "leadgen_health");
            Assert.Equal("ok", health.RootElement.GetProperty("status").GetString());

            using var campaigns = await RunMcpToolAsync(databasePath, "list_campaigns");
            Assert.Contains(campaigns.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == campaignId);

            using var leads = await RunMcpToolAsync(databasePath, "search_leads", "--query", "MCP Smoke");
            Assert.Contains(leads.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == leadId);

            using var dossier = await RunMcpToolAsync(databasePath, "get_lead_dossier", "--leadId", leadId.ToString());
            Assert.Contains("MCP smoke dossier", dossier.RootElement.GetProperty("dossierMarkdown").GetString());

            using var note = await RunMcpToolAsync(
                databasePath,
                "add_lead_note",
                "--leadId",
                leadId.ToString(),
                "--body",
                "MCP smoke note");
            var noteId = note.RootElement.GetProperty("id").GetGuid();

            using var status = await RunMcpToolAsync(
                databasePath,
                "update_lead_status",
                "--leadId",
                leadId.ToString(),
                "--status",
                "Reviewed");
            Assert.Equal("Reviewed", status.RootElement.GetProperty("status").GetString());

            await using var db = CreateContext(databasePath);
            var savedLead = await db.Leads.Include(lead => lead.Notes).FirstAsync(lead => lead.Id == leadId);
            Assert.Equal(LeadStatus.Reviewed, savedLead.Status);
            Assert.Contains(savedLead.Notes, item => item.Id == noteId && item.Body == "MCP smoke note");
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task McpStdio_InitializesAndExposesToolsForIdeClients()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"leadgen-mcp-stdio-{Guid.NewGuid():N}.db");
        Process? process = null;
        try
        {
            var campaignId = Guid.NewGuid();
            var leadId = Guid.NewGuid();
            await SeedDatabaseAsync(databasePath, campaignId, leadId);
            process = StartMcpServer(databasePath);

            await WriteRpcAsync(process, new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = "2025-06-18",
                    capabilities = new { },
                    clientInfo = new
                    {
                        name = "LeadGen.Tests",
                        version = "1.0.0"
                    }
                }
            });

            using var initialize = await ReadRpcAsync(process);
            Assert.Equal(1, initialize.RootElement.GetProperty("id").GetInt32());
            Assert.Equal("leadgen", initialize.RootElement
                .GetProperty("result")
                .GetProperty("serverInfo")
                .GetProperty("name")
                .GetString());

            await WriteRpcAsync(process, new
            {
                jsonrpc = "2.0",
                method = "notifications/initialized"
            });
            await WriteRpcAsync(process, new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "tools/list"
            });

            using var tools = await ReadRpcAsync(process);
            var toolList = tools.RootElement.GetProperty("result").GetProperty("tools").EnumerateArray();
            Assert.Contains(toolList, tool => tool.GetProperty("name").GetString() == "list_campaigns");

            await WriteRpcAsync(process, new
            {
                jsonrpc = "2.0",
                id = 3,
                method = "tools/call",
                @params = new
                {
                    name = "list_campaigns",
                    arguments = new { }
                }
            });

            using var call = await ReadRpcAsync(process);
            var result = call.RootElement.GetProperty("result");
            Assert.False(result.GetProperty("isError").GetBoolean());
            var text = result.GetProperty("content")[0].GetProperty("text").GetString();
            using var campaigns = JsonDocument.Parse(text!);
            Assert.Contains(campaigns.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == campaignId);
        }
        finally
        {
            if (process is not null)
            {
                await StopMcpServerAsync(process);
            }

            DeleteDatabaseFiles(databasePath);
        }
    }

    private static async Task SeedDatabaseAsync(string databasePath, Guid campaignId, Guid leadId)
    {
        await using var db = CreateContext(databasePath);
        await db.Database.MigrateAsync();

        var now = DateTime.UtcNow;
        db.Campaigns.Add(new Campaign
        {
            Id = campaignId,
            Name = "MCP Smoke Campaign",
            BusinessName = "MCP Smoke Studio",
            BusinessDescription = "We help local service businesses capture inquiries.",
            TargetGeography = "Croatia",
            IcpJson = "{\"buyerTypes\":[\"Local service businesses\"],\"targetIndustries\":[\"Professional services\"]}",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        db.Leads.Add(new Lead
        {
            Id = leadId,
            CampaignId = campaignId,
            CompanyName = "MCP Smoke Lead",
            Domain = "mcp-smoke.example",
            WebsiteUrl = "https://mcp-smoke.example",
            Industry = "Professional services",
            Location = "Croatia",
            FitScore = 88,
            ConfidenceScore = 81,
            Status = LeadStatus.New,
            MatchReasonsJson = "[\"MCP smoke reason\"]",
            EvidenceJson = "[{\"title\":\"MCP smoke evidence\",\"url\":\"https://mcp-smoke.example\",\"quoteOrSummary\":\"MCP smoke source\"}]",
            DossierMarkdown = "MCP smoke dossier.",
            SuggestedOutreachAngle = "Use this lead to verify MCP read/write tools.",
            DedupeKey = "domain:mcp-smoke.example",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        await db.SaveChangesAsync();
    }

    private static LeadGenDbContext CreateContext(string databasePath)
    {
        var options = new DbContextOptionsBuilder<LeadGenDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;
        return new LeadGenDbContext(options);
    }

    private static async Task<JsonDocument> RunMcpToolAsync(string databasePath, string tool, params string[] args)
    {
        var root = FindRepositoryRoot();
        var mcpDll = FindMcpDll(root);
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add(mcpDll);
        startInfo.ArgumentList.Add("--tool");
        startInfo.ArgumentList.Add(tool);
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        startInfo.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={databasePath}";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start LeadGen.Mcp.");
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"MCP tool '{tool}' failed with exit code {process.ExitCode}: {error}");
        }

        return JsonDocument.Parse(output);
    }

    private static Process StartMcpServer(string databasePath)
    {
        var root = FindRepositoryRoot();
        var mcpDll = FindMcpDll(root);
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add(mcpDll);
        startInfo.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={databasePath}";

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start LeadGen.Mcp.");
    }

    private static async Task WriteRpcAsync(Process process, object request)
    {
        await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request));
        await process.StandardInput.FlushAsync();
    }

    private static async Task<JsonDocument> ReadRpcAsync(Process process)
    {
        var line = await process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
        if (line is null)
        {
            throw new InvalidOperationException("MCP server closed stdout before returning a response.");
        }

        return JsonDocument.Parse(line);
    }

    private static async Task StopMcpServerAsync(Process process)
    {
        try
        {
            process.StandardInput.Close();
        }
        catch (InvalidOperationException)
        {
        }

        if (!process.HasExited)
        {
            try
            {
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch (TimeoutException)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }

        process.Dispose();
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = databasePath + suffix;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "leadgen.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static string FindMcpDll(string root)
    {
        var projectDir = Path.Combine(root, "src", "LeadGen.Mcp", "bin");
        var dll = Directory.Exists(projectDir)
            ? Directory.GetFiles(projectDir, "LeadGen.Mcp.dll", SearchOption.AllDirectories)
                .OrderByDescending(path => File.GetLastWriteTimeUtc(path))
                .FirstOrDefault()
            : null;

        return dll ?? throw new FileNotFoundException("LeadGen.Mcp.dll was not found. Build LeadGen.sln before running tests.");
    }
}
