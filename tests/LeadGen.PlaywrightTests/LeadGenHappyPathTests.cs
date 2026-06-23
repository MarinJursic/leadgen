using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.Playwright;

namespace LeadGen.PlaywrightTests;

public sealed class LeadGenHappyPathTests
{
    [RealProviderFact]
    public async Task LeadGenHappyPath_10Steps()
    {
        await using var app = await TestWebApp.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = BrowserPaths.FindChromiumExecutable()
        });
        var page = await browser.NewPageAsync();

        await page.GotoAsync(app.BaseUrl);
        await ExpectTextAsync(page, "Lead discovery desk");

        await page.GetByRole(AriaRole.Link, new() { Name = "Campaigns" }).ClickAsync();
        await ExpectTextAsync(page, "Business profiles");

        await page.GetByRole(AriaRole.Link, new() { Name = "Create campaign" }).ClickAsync();
        await page.GetByLabel("Business name").FillAsync("Playwright Studio");
        await page.GetByLabel("Website URL").FillAsync("https://playwright-studio.example");
        await page.GetByLabel("Business location").FillAsync("Croatia");
        await page.GetByLabel("What the business does").FillAsync("We build conversion-focused websites for private clinics in Croatia. The AI should infer likely buyers such as dental clinics, private practices, and healthcare service businesses from this offer.");

        await page.GetByRole(AriaRole.Button, new() { Name = "Create campaign" }).ClickAsync();
        await ExpectTextAsync(page, "Playwright Studio lead search");

        await page.GetByRole(AriaRole.Button, new() { Name = "Find Leads" }).ClickAsync();
        await ExpectTextAsync(page, "Discovery graph");
        await ExpectRunCompletedWithLeadsAsync(page);

        await page.GetByRole(AriaRole.Button, new() { Name = "Open run navigation" }).ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "Lead list" }).ClickAsync();
        await ExpectTextAsync(page, "Dossiers");
        var firstLeadLink = page.Locator(".lead-card h3 a").First;
        var leadName = await firstLeadLink.InnerTextAsync();

        await page.Locator("form.global-search input[name='q']").FillAsync(leadName);
        await page.Locator("form.global-search button").ClickAsync();
        await ExpectTextAsync(page, "Global search");

        await page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex(Regex.Escape(leadName)) }).First.ClickAsync();
        await ExpectTextAsync(page, "Lead dossier");

        await page.Locator("#noteBody").FillAsync("Playwright happy path note");
        await page.GetByRole(AriaRole.Button, new() { Name = "Add note" }).ClickAsync();
        await ExpectTextAsync(page, "Playwright happy path note");
    }

    [Fact]
    public async Task MobileSmoke_390px_MainPagesDoNotOverflow()
    {
        await using var app = await TestWebApp.StartAsync();
        var leadName = await CreateManualLeadAsync(app.BaseUrl, "Mobile Smoke Lead");
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = BrowserPaths.FindChromiumExecutable()
        });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 390, Height = 844 }
        });

        await page.GotoAsync(app.BaseUrl);
        await ExpectNoHorizontalOverflowAsync(page);

        await page.GetByRole(AriaRole.Button, new() { Name = "Toggle navigation" }).ClickAsync();
        await ExpectVisibleAsync(page.Locator("form.global-search input[name='q']"));
        await page.GetByRole(AriaRole.Link, new() { Name = "Campaigns" }).ClickAsync();
        await ExpectNoHorizontalOverflowAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = "Create campaign" }).ClickAsync();
        await ExpectTextAsync(page, "Create campaign");
        await ExpectNoHorizontalOverflowAsync(page);

        await page.GotoAsync($"{app.BaseUrl}/Leads");
        await ExpectTextAsync(page, "Dossiers");
        await ExpectTextAsync(page, leadName);
        await ExpectNoHorizontalOverflowAsync(page);

        await page.GetByRole(AriaRole.Link, new() { Name = leadName }).ClickAsync();
        await ExpectTextAsync(page, "Lead dossier");
        await ExpectNoHorizontalOverflowAsync(page);
    }

    [Fact]
    public async Task LeadsIndex_GroupsLeadsByCampaign()
    {
        await using var app = await TestWebApp.StartAsync();
        await CreateManualLeadAsync(
            app.BaseUrl,
            "Dental Alpha",
            "Clinic Website Campaign",
            "Clinic Sites Studio",
            fitScore: 82);
        await CreateManualLeadAsync(
            app.BaseUrl,
            "Event Hall Beta",
            "Venue Booking Campaign",
            "Venue Booking Studio",
            fitScore: 91);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = BrowserPaths.FindChromiumExecutable()
        });
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{app.BaseUrl}/Leads");
        await ExpectTextAsync(page, "Dossiers by campaign");
        await ExpectTextAsync(page, "Clinic Website Campaign");
        await ExpectTextAsync(page, "Venue Booking Campaign");
        await ExpectTextAsync(page, "Dental Alpha");
        await ExpectTextAsync(page, "Event Hall Beta");
        Assert.Equal(2, await page.Locator(".lead-campaign-section").CountAsync());

        var bodyText = await page.Locator("body").InnerTextAsync();
        Assert.True(bodyText.IndexOf("Clinic Website Campaign", StringComparison.Ordinal) < bodyText.IndexOf("Dental Alpha", StringComparison.Ordinal));
        Assert.True(bodyText.IndexOf("Venue Booking Campaign", StringComparison.Ordinal) < bodyText.IndexOf("Event Hall Beta", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CampaignCreate_ShowsLoadingStateWhileSubmitting()
    {
        await using var app = await TestWebApp.StartAsync();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = BrowserPaths.FindChromiumExecutable()
        });
        var page = await browser.NewPageAsync();

        await page.GotoAsync($"{app.BaseUrl}/Campaigns/Create");

        await page.GetByLabel("Business name").FillAsync("Loading State Studio");
        await page.GetByLabel("Website URL").FillAsync("https://loading-state.example");
        await page.GetByLabel("Business location").FillAsync("Croatia");
        await page.GetByLabel("What the business does").FillAsync("We build booking websites for local event venues and service providers.");

        var submitButton = page.Locator("[data-submit-button]");
        var stopwatch = Stopwatch.StartNew();
        await submitButton.EvaluateAsync("button => button.click()");

        await ExpectTextAsync(page, "Creating...");
        await ExpectTextAsync(page, "Saving business profile...");
        await Assertions.Expect(page.Locator("#campaignCreateProgress")).ToBeVisibleAsync();
        await Assertions.Expect(submitButton).ToBeDisabledAsync();
        await page.WaitForURLAsync(new Regex("/Campaigns/Details/[0-9a-fA-F-]+$"));
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds >= 750, $"Expected submit delay to be visible, but it was {stopwatch.ElapsedMilliseconds} ms.");
        await ExpectTextAsync(page, "Loading State Studio lead search");
        await ExpectTextAsync(page, "Campaign saved.");
        await Assertions.Expect(page.Locator(".app-status-toast")).ToBeVisibleAsync();
        Assert.Equal(0, await page.Locator(".alert-info").CountAsync());
    }

    [Fact]
    public async Task RunDetailsGraph_RendersFromProgressLogs()
    {
        await using var app = await TestWebApp.StartAsync();
        var runId = await CreateGraphRunAsync(app.DatabasePath);
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = BrowserPaths.FindChromiumExecutable()
        });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 820 }
        });

        await page.GotoAsync($"{app.BaseUrl}/Runs/Details/{runId}");
        await ExpectTextAsync(page, "Discovery graph");
        await ExpectTextAsync(page, "Base -> queries -> sites -> pages -> leads -> contacts");
        await ExpectTextAsync(page, "In progress");
        await ExpectTextAsync(page, "Waiting");
        await ExpectTextAsync(page, "info@clinic-alpha.example");
        await ExpectTextAsync(page, "2 sites scanned / 1 lead saved");
        await ExpectVisibleAsync(page.Locator(".run-map-controls"));
        Assert.Equal(0, await page.Locator(".run-overview-card").CountAsync());
        await page.WaitForTimeoutAsync(1_000);

        Assert.True(await page.Locator(".run-graph-node").CountAsync() >= 6);
        Assert.True(await page.Locator(".run-graph-edge").CountAsync() >= 5);
        Assert.True(await page.Locator(".run-graph-status-active").CountAsync() >= 1);
        Assert.True(await page.Locator(".run-graph-status-waiting").CountAsync() >= 1);
        Assert.Equal(1, await page.Locator(".run-graph-node-result").CountAsync());
        Assert.Equal(0, await page.Locator(".run-graph-node-candidate").CountAsync());
        Assert.Equal(0, await page.Locator(".run-graph-node-extract").CountAsync());
        Assert.Equal(0, await page.Locator(".run-graph-node-page").CountAsync());
        Assert.True(await page.EvaluateAsync<bool>(
            """
            () => {
                const frame = document.querySelector('.run-graph-frame').getBoundingClientRect();
                const root = document.querySelector('.run-graph-node-base').getBoundingClientRect();
                const rootX = root.left + root.width / 2;
                const rootY = root.top + root.height / 2;
                const frameX = frame.left + frame.width / 2;
                const frameY = frame.top + frame.height / 2;
                return Math.abs(rootX - frameX) < 90 && Math.abs(rootY - frameY) < 90;
            }
            """));
        Assert.True(await page.EvaluateAsync<bool>(
            """
            () => {
                const centerX = selector => {
                    const rect = document.querySelector(selector).getBoundingClientRect();
                    return rect.left + rect.width / 2;
                };
                const root = centerX('.run-graph-node-base');
                const query = centerX('.run-graph-node-query');
                const site = centerX('.run-graph-node-result');
                const lead = centerX('.run-graph-node-lead');
                const contact = centerX('.run-graph-node-contact');
                return root < query && query < site && site < lead && lead < contact;
            }
            """));
        Assert.True(await page.EvaluateAsync<bool>(
            """
            () => {
                const nodes = [...document.querySelectorAll('.run-graph-node')]
                    .map(node => node.getBoundingClientRect());
                for (let i = 0; i < nodes.length; i += 1) {
                    for (let j = i + 1; j < nodes.length; j += 1) {
                        const a = nodes[i];
                        const b = nodes[j];
                        const separated = a.right <= b.left + 2
                            || b.right <= a.left + 2
                            || a.bottom <= b.top + 2
                            || b.bottom <= a.top + 2;
                        if (!separated) {
                            return false;
                        }
                    }
                }
                return true;
            }
            """));
        Assert.True(await page.EvaluateAsync<bool>(
            """
            () => {
                const center = selector => {
                    const rect = document.querySelector(selector).getBoundingClientRect();
                    return { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
                };
                const query = center('.run-graph-node-query');
                const site = center('.run-graph-node-result');
                const lead = center('.run-graph-node-lead');
                const contact = center('.run-graph-node-contact');
                const distance = (a, b) => Math.hypot(a.x - b.x, a.y - b.y);
                return distance(query, site) < 430
                    && distance(site, lead) < 430
                    && distance(lead, contact) < 430;
            }
            """));
        await page.Locator("#runZoomIn").ClickAsync();
        await Assertions.Expect(page.Locator("#runZoomValue")).ToHaveTextAsync("116%");
        await page.Locator("#runZoomReset").ClickAsync();
        var beforePanX = await page.Locator(".run-graph-node-base").EvaluateAsync<double>("node => node.getBoundingClientRect().left");
        var dragPoint = await page.EvaluateAsync<float[]>(
            """
            () => {
                const frame = document.querySelector('.run-graph-frame').getBoundingClientRect();
                const blockers = [...document.querySelectorAll('.run-graph-node, .run-map-controls, .run-graph-inspector, .run-map-heading, .run-map-nav, .run-map-status, .run-graph-legend')]
                    .map(element => element.getBoundingClientRect());
                for (const y of [frame.top + frame.height * 0.72, frame.top + frame.height * 0.55, frame.top + frame.height * 0.35]) {
                    for (const x of [frame.left + 80, frame.left + frame.width - 90, frame.left + frame.width * 0.22, frame.left + frame.width * 0.78]) {
                        const blocked = blockers.some(rect => x >= rect.left - 8 && x <= rect.right + 8 && y >= rect.top - 8 && y <= rect.bottom + 8);
                        if (!blocked) {
                            return [x, y];
                        }
                    }
                }
                return [frame.left + frame.width - 80, frame.top + 120];
            }
            """);
        await page.Mouse.MoveAsync(dragPoint[0], dragPoint[1]);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(dragPoint[0] - 90, dragPoint[1] - 50, new MouseMoveOptions { Steps = 8 });
        await page.Mouse.UpAsync();
        var afterPanX = await page.Locator(".run-graph-node-base").EvaluateAsync<double>("node => node.getBoundingClientRect().left");
        Assert.True(Math.Abs(afterPanX - beforePanX) > 40);
        var graphHeight = await page.Locator(".run-graph-frame").EvaluateAsync<double>("element => element.getBoundingClientRect().height");
        Assert.True(graphHeight >= 420);
        await ExpectNoHorizontalOverflowAsync(page);
    }

    private static async Task<string> CreateManualLeadAsync(
        string baseUrl,
        string companyName,
        string campaignName = "Mobile Smoke Campaign",
        string businessName = "Mobile Smoke Studio",
        int fitScore = 76)
    {
        using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        var campaignResponse = await client.PostAsJsonAsync("/api/campaigns", new
        {
            name = campaignName,
            businessName,
            websiteUrl = "https://mobile-smoke.example",
            businessDescription = "Manual campaign for mobile smoke verification.",
            targetGeography = "Croatia",
            targetCustomers = "Manual verification businesses",
            exclusions = "No login-gated sites"
        });
        campaignResponse.EnsureSuccessStatusCode();
        using var campaignDocument = await JsonDocument.ParseAsync(await campaignResponse.Content.ReadAsStreamAsync());
        var campaignId = campaignDocument.RootElement.GetProperty("id").GetGuid();

        var leadResponse = await client.PostAsJsonAsync("/api/leads", new
        {
            campaignId,
            companyName,
            domain = "mobile-smoke.example",
            websiteUrl = "https://mobile-smoke.example",
            industry = "Manual verification",
            location = "Zagreb, Croatia",
            fitScore,
            confidenceScore = 70,
            status = "New",
            matchReasonsJson = "[\"Manual smoke reason\"]",
            evidenceJson = "[{\"title\":\"Manual smoke evidence\",\"url\":\"https://mobile-smoke.example\",\"quoteOrSummary\":\"Manual source\"}]",
            dossierMarkdown = "Manual lead dossier for mobile smoke verification.",
            suggestedOutreachAngle = "Review manually."
        });
        leadResponse.EnsureSuccessStatusCode();
        return companyName;
    }

    private static async Task<Guid> CreateGraphRunAsync(string databasePath)
    {
        var campaignId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var queriesJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                query = "dental clinics Croatia contact email",
                purpose = "Find clinic contact pages"
            }
        });
        var logsJson = JsonSerializer.Serialize(new[]
        {
            "Queued by web request",
            "Graph|Base|Graph Check Campaign",
            "Queued -> Running",
            "Graph|Query|dental clinics Croatia contact email|Find clinic contact pages",
            "Graph|Result|dental clinics Croatia contact email|clinic-alpha.example|Clinic Alpha Contact|https://clinic-alpha.example/contact",
            "Graph|Result|dental clinics Croatia contact email|clinic-beta.example|Clinic Beta Contact|https://clinic-beta.example/contact",
            "Graph|Candidate|clinic-alpha.example|https://clinic-alpha.example|92",
            "Graph|Candidate|clinic-beta.example|https://clinic-beta.example|76",
            "Graph|Extract|clinic-alpha.example|https://clinic-alpha.example/contact",
            "Graph|Extract|clinic-beta.example|https://clinic-beta.example/contact",
            "Graph|Page|clinic-alpha.example|Clinic Alpha Contact|https://clinic-alpha.example/contact",
            "Graph|Page|clinic-beta.example|Clinic Beta Contact|https://clinic-beta.example/contact",
            "Graph|NoContact|clinic-beta.example|Clinic Beta",
            "Graph|Lead|Clinic Alpha|clinic-alpha.example|https://clinic-alpha.example|90",
            "Graph|Contact|Clinic Alpha|Email|info@clinic-alpha.example|https://clinic-alpha.example/contact",
            "Saved exact company lead with email: Clinic Alpha"
        });

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using (var campaign = connection.CreateCommand())
        {
            campaign.CommandText = """
                INSERT INTO Campaigns
                    (Id, Name, BusinessName, WebsiteUrl, BusinessDescription, TargetGeography, TargetCustomers, Exclusions, IcpJson, CreatedAtUtc, UpdatedAtUtc)
                VALUES
                    ($id, $name, $businessName, $websiteUrl, $description, $geography, $customers, $exclusions, NULL, $created, $updated);
                """;
            campaign.Parameters.AddWithValue("$id", campaignId.ToString().ToUpperInvariant());
            campaign.Parameters.AddWithValue("$name", "Graph Check Campaign");
            campaign.Parameters.AddWithValue("$businessName", "Graph Check Studio");
            campaign.Parameters.AddWithValue("$websiteUrl", "https://graph-check.example");
            campaign.Parameters.AddWithValue("$description", "UI graph verification campaign.");
            campaign.Parameters.AddWithValue("$geography", "Croatia");
            campaign.Parameters.AddWithValue("$customers", "Dental clinics");
            campaign.Parameters.AddWithValue("$exclusions", "No login-gated sources");
            campaign.Parameters.AddWithValue("$created", now);
            campaign.Parameters.AddWithValue("$updated", now);
            await campaign.ExecuteNonQueryAsync();
        }

        await using (var run = connection.CreateCommand())
        {
            run.CommandText = """
                INSERT INTO LeadSearchRuns
                    (Id, CampaignId, Status, RequestedLeadCount, SearchQueriesJson, StartedAtUtc, CompletedAtUtc, ErrorMessage, EstimatedCostUsd, LogsJson)
                VALUES
                    ($id, $campaignId, $status, $requestedLeadCount, $queries, $started, NULL, NULL, $cost, $logs);
                """;
            run.Parameters.AddWithValue("$id", runId.ToString().ToUpperInvariant());
            run.Parameters.AddWithValue("$campaignId", campaignId.ToString().ToUpperInvariant());
            run.Parameters.AddWithValue("$status", "Running");
            run.Parameters.AddWithValue("$requestedLeadCount", 5);
            run.Parameters.AddWithValue("$queries", queriesJson);
            run.Parameters.AddWithValue("$started", now);
            run.Parameters.AddWithValue("$cost", "0.0");
            run.Parameters.AddWithValue("$logs", logsJson);
            await run.ExecuteNonQueryAsync();
        }

        return runId;
    }

    private static async Task ExpectTextAsync(IPage page, string text, int timeout = 10_000)
    {
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync(text, new LocatorAssertionsToContainTextOptions
        {
            Timeout = timeout
        });
    }

    private static async Task ExpectVisibleAsync(ILocator locator)
    {
        await Assertions.Expect(locator).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 10_000
        });
    }

    private static async Task ExpectRunCompletedWithLeadsAsync(IPage page)
    {
        await Assertions.Expect(page.Locator("#runStatusValue")).ToHaveTextAsync("Completed", new LocatorAssertionsToHaveTextOptions
        {
            Timeout = 180_000
        });

        await page.WaitForFunctionAsync(
            "() => Number.parseInt(document.querySelector('#runLeadCount')?.textContent || '0', 10) > 0",
            new PageWaitForFunctionOptions
            {
                Timeout = 180_000
            });
    }

    private static async Task ExpectNoHorizontalOverflowAsync(IPage page)
    {
        var hasOverflow = await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");
        Assert.False(hasOverflow);
    }
}

internal sealed class TestWebApp : IAsyncDisposable
{
    private readonly Process _process;
    private readonly string _databasePath;

    private TestWebApp(Process process, string baseUrl, string databasePath)
    {
        _process = process;
        BaseUrl = baseUrl;
        _databasePath = databasePath;
    }

    public string BaseUrl { get; }

    public string DatabasePath => _databasePath;

    public static async Task<TestWebApp> StartAsync()
    {
        var root = FindRepositoryRoot();
        var webProject = Path.Combine(root, "src", "LeadGen.Web", "LeadGen.Web.csproj");
        var port = FindFreePort();
        var baseUrl = $"http://127.0.0.1:{port}";
        var databasePath = Path.Combine(Path.GetTempPath(), $"leadgen-playwright-{Guid.NewGuid():N}.db");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Path.GetDirectoryName(webProject)!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.StartInfo.ArgumentList.Add("run");
        process.StartInfo.ArgumentList.Add("--no-build");
        process.StartInfo.ArgumentList.Add("--project");
        process.StartInfo.ArgumentList.Add(webProject);
        process.StartInfo.ArgumentList.Add("--urls");
        process.StartInfo.ArgumentList.Add(baseUrl);
        process.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
        process.StartInfo.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={databasePath}";
        process.StartInfo.Environment["LeadGen__EnableAdminLogViewer"] = "true";
        process.StartInfo.Environment["LeadGen__MaxLeadsPerRun"] = "10";

        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        if (!process.Start())
        {
            throw new InvalidOperationException("Could not start LeadGen.Web.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var client = new HttpClient();
        var deadline = DateTime.UtcNow.AddSeconds(45);
        while (DateTime.UtcNow < deadline)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"LeadGen.Web exited early with code {process.ExitCode}.");
            }

            try
            {
                using var response = await client.GetAsync($"{baseUrl}/api/health");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new TestWebApp(process, baseUrl, databasePath);
                }
            }
            catch
            {
                await Task.Delay(250);
            }
        }

        process.Kill(entireProcessTree: true);
        throw new TimeoutException("LeadGen.Web did not become healthy in time.");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }

        _process.Dispose();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var path = _databasePath + suffix;
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
            if (File.Exists(Path.Combine(directory.FullName, "LeadGen.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static int FindFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

internal static class BrowserPaths
{
    public static string? FindChromiumExecutable()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var cache = Path.Combine(home, "Library", "Caches", "ms-playwright");
        if (!Directory.Exists(cache))
        {
            return null;
        }

        return Directory.GetFiles(cache, "chrome-headless-shell", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(cache, "Chromium", SearchOption.AllDirectories))
            .OrderByDescending(path => path.Contains("chromium_headless_shell", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();
    }
}
