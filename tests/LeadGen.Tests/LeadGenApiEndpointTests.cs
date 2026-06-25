using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LeadGen.Tests;

public sealed class LeadGenApiEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();

        using var document = await GetJsonAsync(client, "/api/health");

        Assert.Equal("OK", document.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Campaigns_Crud_Works()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "API Campaign CRUD");

        var get = await client.GetAsync($"/api/campaigns/{campaignId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var update = await client.PutAsJsonAsync($"/api/campaigns/{campaignId}", new
        {
            name = "API Campaign CRUD Updated",
            businessName = "API Studio Updated",
            websiteUrl = "https://api-studio.example",
            businessDescription = "Updated lead generation API test campaign.",
            targetGeography = "Croatia",
            targetCustomers = "Private clinics",
            exclusions = "No login-gated sites",
            icpJson = "{\"summary\":\"updated\"}"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        using var list = await GetJsonAsync(client, "/api/campaigns");
        Assert.Contains(list.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == campaignId);

        var delete = await client.DeleteAsync($"/api/campaigns/{campaignId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var missing = await client.GetAsync($"/api/campaigns/{campaignId}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

#if REAL_PROVIDER_TESTS
    [RealProviderFact]
    public async Task GenerateIcp_WithRealProvider_ReturnsJson()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "ICP Campaign");

        using var icp = await PostJsonAsync(client, $"/api/campaigns/{campaignId}/generate-icp", new { });

        Assert.Equal(campaignId, icp.RootElement.GetProperty("campaignId").GetGuid());
        Assert.True(icp.RootElement.GetProperty("icp").TryGetProperty("summary", out _));
    }

    [RealProviderFact]
    public async Task StartRun_WithRealProviders_CreatesLeads()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "Run Campaign");

        using var run = await PostJsonAsync(client, $"/api/campaigns/{campaignId}/runs", new { requestedLeadCount = 3 });
        var runId = run.RootElement.GetProperty("id").GetGuid();
        Assert.Equal("Completed", run.RootElement.GetProperty("status").GetString());

        using var leads = await GetJsonAsync(client, $"/api/leads?campaignId={campaignId}");
        var leadItems = leads.RootElement.EnumerateArray().ToList();
        Assert.True(leadItems.Count >= 1);
        Assert.All(leadItems, lead =>
        {
            Assert.False(string.IsNullOrWhiteSpace(lead.GetProperty("dossierMarkdown").GetString()));
            Assert.NotEqual("[]", lead.GetProperty("matchReasonsJson").GetString());
            Assert.NotEqual("[]", lead.GetProperty("evidenceJson").GetString());
            Assert.NotEmpty(lead.GetProperty("contacts").EnumerateArray());
        });

        using var runStatus = await GetJsonAsync(client, $"/api/runs/{runId}");
        Assert.Equal("Completed", runStatus.RootElement.GetProperty("status").GetString());
    }

    [RealProviderFact]
    public async Task RunStatus_ReturnsCompleted()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "Run Status Campaign");
        using var created = await PostJsonAsync(client, $"/api/campaigns/{campaignId}/runs", new { requestedLeadCount = 1 });
        var runId = created.RootElement.GetProperty("id").GetGuid();

        using var run = await GetJsonAsync(client, $"/api/runs/{runId}");

        Assert.Equal("Completed", run.RootElement.GetProperty("status").GetString());
        Assert.True(run.RootElement.GetProperty("leadCount").GetInt32() >= 1);
    }
#endif

    [Fact]
    public async Task Leads_Crud_Works()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "Lead CRUD Campaign");
        var leadId = await CreateLeadAsync(client, campaignId, "Manual API Lead");

        var get = await client.GetAsync($"/api/leads/{leadId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var update = await client.PutAsJsonAsync($"/api/leads/{leadId}", NewLeadPayload(campaignId, "Manual API Lead Updated", "Reviewed"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/leads/{leadId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task Contacts_Crud_Works()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "Contact CRUD Campaign");
        var leadId = await CreateLeadAsync(client, campaignId, "Contact Lead");

        using var created = await PostJsonAsync(client, $"/api/leads/{leadId}/contacts", new
        {
            type = "ContactPage",
            value = "https://contact-lead.example/contact",
            sourceUrl = "https://contact-lead.example",
            confidenceScore = 75,
            isVerified = false
        });
        var contactId = created.RootElement.GetProperty("id").GetGuid();

        var update = await client.PutAsJsonAsync($"/api/contacts/{contactId}", new
        {
            type = "Email",
            value = "hello@contact-lead.example",
            sourceUrl = "https://contact-lead.example",
            confidenceScore = 80,
            isVerified = false
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/contacts/{contactId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task Notes_Crud_Works()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "Note CRUD Campaign");
        var leadId = await CreateLeadAsync(client, campaignId, "Note Lead");

        using var created = await PostJsonAsync(client, $"/api/leads/{leadId}/notes", new { body = "Initial API note" });
        var noteId = created.RootElement.GetProperty("id").GetGuid();

        var update = await client.PutAsJsonAsync($"/api/notes/{noteId}", new { body = "Updated API note" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/notes/{noteId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task GlobalSearch_ReturnsMenuAndDataResults()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();
        var campaignId = await CreateCampaignAsync(client, "Searchable Campaign");
        await CreateLeadAsync(client, campaignId, "Searchable Alpha Clinic");

        using var menu = await GetJsonAsync(client, "/api/search?q=Campaigns");
        Assert.Contains(menu.RootElement.EnumerateArray(), item => item.GetProperty("type").GetString() == "Menu");

        using var data = await GetJsonAsync(client, "/api/search?q=Alpha");
        Assert.Contains(data.RootElement.EnumerateArray(), item => item.GetProperty("type").GetString() == "Lead");
    }

    [Fact]
    public async Task Logs_ReturnsSafeLines()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();

        var response = await client.GetAsync("/api/logs?take=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Errors_IncludeCorrelationId()
    {
        using var factory = new LeadGenApiTestFactory();
        using var client = factory.CreateLeadGenClient();

        var response = await client.GetAsync($"/api/campaigns/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var correlationId = document.RootElement.GetProperty("error").GetProperty("correlationId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    private static async Task<Guid> CreateCampaignAsync(HttpClient client, string name)
    {
        using var created = await PostJsonAsync(client, "/api/campaigns", new
        {
            name,
            businessName = "API Studio",
            websiteUrl = "https://api-studio.example",
            businessDescription = "We build conversion-focused websites for private clinics in Croatia.",
            targetGeography = "Croatia",
            targetCustomers = "Dental clinics and private practices",
            exclusions = "No LinkedIn-only sources"
        });
        return created.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateLeadAsync(HttpClient client, Guid campaignId, string companyName)
    {
        using var created = await PostJsonAsync(client, "/api/leads", NewLeadPayload(campaignId, companyName, "New"));
        return created.RootElement.GetProperty("id").GetGuid();
    }

    private static object NewLeadPayload(Guid campaignId, string companyName, string status)
    {
        return new
        {
            campaignId,
            companyName,
            domain = $"{companyName.ToLowerInvariant().Replace(" ", "-")}.example",
            websiteUrl = "https://manual-lead.example",
            industry = "Dental clinic",
            location = "Zagreb, Croatia",
            fitScore = 77,
            confidenceScore = 70,
            status,
            matchReasonsJson = "[\"Manual API reason\"]",
            evidenceJson = "[{\"title\":\"Manual evidence\",\"url\":\"https://manual-lead.example\",\"quoteOrSummary\":\"Manual source\"}]",
            dossierMarkdown = "Manual lead dossier for API tests.",
            suggestedOutreachAngle = "Offer a manual website audit."
        };
    }

    private static async Task<JsonDocument> GetJsonAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }

    private static async Task<JsonDocument> PostJsonAsync(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body, JsonOptions);
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }
}
