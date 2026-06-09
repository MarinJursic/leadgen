using Leadgen.Model.Enums;
using leadgen.Data;
using leadgen.Models.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace leadgen.Tests;

public sealed class LeadgenApiCrudTests
{
    public static IEnumerable<object[]> CrudScenarios()
    {
        return Scenarios.Select(scenario => new object[] { scenario });
    }

    [Theory]
    [MemberData(nameof(CrudScenarios))]
    public async Task ApiEndpoint_SupportsFullCrud(CrudScenario scenario)
    {
        using var factory = new LeadgenApiTestFactory();
        using var client = factory.CreateAuthenticatedClient();
        var seed = await GetSeedGraphAsync(factory);

        var createResponse = await client.PostAsync(scenario.Endpoint, JsonBody(scenario.Create(seed)));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdId = await ReadIdAsync(createResponse);

        var listResponse = await client.GetAsync($"{scenario.Endpoint}?query={Uri.EscapeDataString(scenario.Query)}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        await AssertArrayContainsIdAsync(listResponse, createdId);

        var getResponse = await client.GetAsync($"{scenario.Endpoint}/{createdId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client.PutAsync($"{scenario.Endpoint}/{createdId}", JsonBody(scenario.Update(seed)));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(createdId, await ReadIdAsync(updateResponse));

        var deleteResponse = await client.DeleteAsync($"{scenario.Endpoint}/{createdId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedGetResponse = await client.GetAsync($"{scenario.Endpoint}/{createdId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedGetResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(CrudScenarios))]
    public async Task ApiEndpoint_ReturnsNotFound_ForMissingIds(CrudScenario scenario)
    {
        using var factory = new LeadgenApiTestFactory();
        using var client = factory.CreateAuthenticatedClient();
        var seed = await GetSeedGraphAsync(factory);
        var missingId = Guid.NewGuid();

        var getResponse = await client.GetAsync($"{scenario.Endpoint}/{missingId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var putResponse = await client.PutAsync($"{scenario.Endpoint}/{missingId}", JsonBody(scenario.Update(seed)));
        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"{scenario.Endpoint}/{missingId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(CrudScenarios))]
    public async Task ApiEndpoint_ReturnsBadRequest_ForInvalidCreatePayload(CrudScenario scenario)
    {
        using var factory = new LeadgenApiTestFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.PostAsync(scenario.Endpoint, JsonBody(new { }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(CrudScenarios))]
    public async Task ApiEndpoint_PublicList_AllowsAnonymousUsers(CrudScenario scenario)
    {
        using var factory = new LeadgenApiTestFactory();
        using var client = factory.CreateAnonymousClient();

        var response = await client.GetAsync(scenario.Endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(CrudScenarios))]
    public async Task ApiEndpoint_ProtectedOperations_RequireAuthentication(CrudScenario scenario)
    {
        using var factory = new LeadgenApiTestFactory();
        using var client = factory.CreateAnonymousClient();
        var missingId = Guid.NewGuid();

        var getResponse = await client.GetAsync($"{scenario.Endpoint}/{missingId}");
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);

        var postResponse = await client.PostAsync(scenario.Endpoint, JsonBody(new { }));
        Assert.Equal(HttpStatusCode.Unauthorized, postResponse.StatusCode);

        var putResponse = await client.PutAsync($"{scenario.Endpoint}/{missingId}", JsonBody(new { }));
        Assert.Equal(HttpStatusCode.Unauthorized, putResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"{scenario.Endpoint}/{missingId}");
        Assert.Equal(HttpStatusCode.Unauthorized, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task ApiEndpoint_Delete_RequiresAdminRole()
    {
        using var factory = new LeadgenApiTestFactory();
        using var client = factory.CreateAuthenticatedClient("Manager");

        var response = await client.DeleteAsync($"/api/missions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static JsonContent JsonBody(object value)
    {
        return JsonContent.Create(value, value.GetType());
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task AssertArrayContainsIdAsync(HttpResponseMessage response, Guid id)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Contains(document.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == id);
    }

    private static async Task<SeedGraph> GetSeedGraphAsync(LeadgenApiTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LeadgenDbContext>();

        var mission = await dbContext.BusinessDnaMissions.AsNoTracking()
            .Include(item => item.Runs)
            .ThenInclude(run => run.TargetCompanies)
            .ThenInclude(company => company.Contacts)
            .FirstAsync(item => item.Runs.Any(run => run.TargetCompanies.Any(company => company.Contacts.Any())));

        var run = mission.Runs.First(item => item.TargetCompanies.Any(company => company.Contacts.Any()));
        var company = run.TargetCompanies.First(item => item.Contacts.Any());
        var contact = company.Contacts.First();
        var agentId = await dbContext.SwarmAgents.AsNoTracking()
            .Select(agent => agent.Id)
            .FirstAsync();

        return new SeedGraph(mission.Id, run.Id, agentId, company.Id, contact.Id);
    }

    private static string Unique(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 13, prefix.Length + 33)];
    }

    private static readonly IReadOnlyList<CrudScenario> Scenarios =
    [
        new(
            "missions",
            "/api/missions",
            "API Mission",
            _ => new BusinessDnaMissionWriteDto
            {
                MissionName = Unique("API Mission"),
                ProductName = "API Product",
                Mechanic = "Detects integration-test opportunities across the leadgen graph.",
                PrimarySurface = "API",
                SurfaceTags = ["api", "integration"],
                Persona = "RevOps lead",
                Villain = "Manual lead research drift",
                Delta = "Higher quality lead dossiers",
                ConfidenceScore = 0.71m,
                CreatedAtUtc = DateTime.UtcNow,
                Status = MissionStatus.ReadyForResearch
            },
            _ => new BusinessDnaMissionWriteDto
            {
                MissionName = Unique("API Mission Updated"),
                ProductName = "API Product Updated",
                Mechanic = "Updates integration-test opportunities across the leadgen graph.",
                PrimarySurface = "Workflow",
                SurfaceTags = ["api", "updated"],
                Persona = "Sales operations lead",
                Villain = "Unverified buyer context",
                Delta = "Cleaner outreach decisions",
                ConfidenceScore = 0.83m,
                CreatedAtUtc = DateTime.UtcNow,
                Status = MissionStatus.Running
            }),
        new(
            "clarification questions",
            "/api/clarification-questions",
            "API Question",
            seed => new ClarificationQuestionWriteDto
            {
                BusinessDnaMissionId = seed.MissionId,
                SlotName = "API Question",
                Prompt = "Which buyer owns this API-tested mission?",
                Reason = "The API needs a concrete clarification gap.",
                IsAnswered = false,
                CreatedAtUtc = DateTime.UtcNow
            },
            seed => new ClarificationQuestionWriteDto
            {
                BusinessDnaMissionId = seed.MissionId,
                SlotName = "API Question Updated",
                Prompt = "Which buyer owns this updated mission?",
                Reason = "The test covers answered question validation.",
                IsAnswered = true,
                Answer = "The RevOps owner owns it.",
                CreatedAtUtc = DateTime.UtcNow,
                AnsweredAtUtc = DateTime.UtcNow.AddMinutes(5)
            }),
        new(
            "mission runs",
            "/api/mission-runs",
            "RUN-API",
            seed => new MissionRunWriteDto
            {
                RunCode = Unique("RUN-API"),
                BusinessDnaMissionId = seed.MissionId,
                StartedAtUtc = DateTime.UtcNow,
                Status = MissionStatus.Running,
                SearchRegion = "API test region",
                TokenBudget = 1200,
                EstimatedCostUsd = 9.25m
            },
            seed => new MissionRunWriteDto
            {
                RunCode = Unique("RUN-API-UPD"),
                BusinessDnaMissionId = seed.MissionId,
                StartedAtUtc = DateTime.UtcNow,
                CompletedAtUtc = DateTime.UtcNow.AddHours(1),
                Status = MissionStatus.Completed,
                SearchRegion = "Updated API test region",
                TokenBudget = 1500,
                EstimatedCostUsd = 12.50m
            }),
        new(
            "assignments",
            "/api/mission-agent-assignments",
            "API Assignment",
            seed => new MissionAgentAssignmentWriteDto
            {
                MissionRunId = seed.RunId,
                SwarmAgentId = seed.AgentId,
                AssignedAtUtc = DateTime.UtcNow,
                Responsibility = "API Assignment coverage",
                TokenBudget = 600,
                Status = MissionStatus.Running
            },
            seed => new MissionAgentAssignmentWriteDto
            {
                MissionRunId = seed.RunId,
                SwarmAgentId = seed.AgentId,
                AssignedAtUtc = DateTime.UtcNow,
                Responsibility = "API Assignment updated coverage",
                TokenBudget = 700,
                Status = MissionStatus.Completed
            }),
        new(
            "swarm agents",
            "/api/swarm-agents",
            "API-AGENT",
            _ => new SwarmAgentWriteDto
            {
                CodeName = Unique("API-AGENT"),
                Role = AgentRole.Scout,
                Provider = "OpenAI",
                Temperature = 0.20m,
                MaxConcurrentTasks = 2,
                IsActive = true,
                LastHeartbeatUtc = DateTime.UtcNow,
                CurrentFocus = "API agent create coverage"
            },
            _ => new SwarmAgentWriteDto
            {
                CodeName = Unique("API-AGENT-UPD"),
                Role = AgentRole.Sentinel,
                Provider = "OpenAI",
                Temperature = 0.30m,
                MaxConcurrentTasks = 3,
                IsActive = true,
                LastHeartbeatUtc = DateTime.UtcNow,
                CurrentFocus = "API agent update coverage"
            }),
        new(
            "target companies",
            "/api/target-companies",
            "API Company",
            seed => new TargetCompanyWriteDto
            {
                MissionRunId = seed.RunId,
                Name = Unique("API Company"),
                Domain = "api-company.example",
                Industry = "Integration testing",
                HeadquartersCity = "Zagreb",
                HeadquartersCountry = "Croatia",
                OrganizationStageLabel = "Lab",
                LastSignalAtUtc = DateTime.UtcNow,
                EmployeeCount = 42,
                IsHeadquartersVerified = true,
                MatchScore = 0.72m
            },
            seed => new TargetCompanyWriteDto
            {
                MissionRunId = seed.RunId,
                Name = Unique("API Company Updated"),
                Domain = "api-company-updated.example",
                Industry = "Automated QA",
                HeadquartersCity = "Split",
                HeadquartersCountry = "Croatia",
                OrganizationStageLabel = "Growth",
                LastSignalAtUtc = DateTime.UtcNow,
                EmployeeCount = 55,
                IsHeadquartersVerified = true,
                MatchScore = 0.86m
            }),
        new(
            "target contacts",
            "/api/target-contacts",
            "API Contact",
            seed => new TargetContactWriteDto
            {
                TargetCompanyId = seed.CompanyId,
                FullName = Unique("API Contact"),
                JobTitle = "Head of API Testing",
                Department = "Engineering",
                Seniority = "Director",
                IsDecisionMaker = true,
                LinkedInUrl = "https://linkedin.example/api-contact",
                XHandle = "@api_contact",
                GitHubUsername = "api-contact",
                OpportunitySummary = "Owns API validation workflows.",
                LastObservedAtUtc = DateTime.UtcNow
            },
            seed => new TargetContactWriteDto
            {
                TargetCompanyId = seed.CompanyId,
                FullName = Unique("API Contact Updated"),
                JobTitle = "VP API Testing",
                Department = "Platform",
                Seniority = "VP",
                IsDecisionMaker = true,
                LinkedInUrl = "https://linkedin.example/api-contact-updated",
                XHandle = "@api_contact_u",
                GitHubUsername = "api-contact-updated",
                OpportunitySummary = "Owns updated API validation workflows.",
                LastObservedAtUtc = DateTime.UtcNow
            }),
        new(
            "contact channels",
            "/api/contact-channels",
            "API channel",
            seed => new ContactChannelWriteDto
            {
                TargetContactId = seed.ContactId,
                Type = ContactChannelType.LinkedIn,
                Value = "https://linkedin.example/in/api-channel",
                IsVerified = true,
                VerifiedAtUtc = DateTime.UtcNow,
                Source = "API channel test",
                ConfidenceScore = 0.81m
            },
            seed => new ContactChannelWriteDto
            {
                TargetContactId = seed.ContactId,
                Type = ContactChannelType.WorkEmail,
                Value = "api.channel@example.test",
                IsVerified = true,
                VerifiedAtUtc = DateTime.UtcNow,
                Source = "API channel updated test",
                ConfidenceScore = 0.91m
            }),
        new(
            "evidence points",
            "/api/evidence-points",
            "API Evidence",
            seed => new EvidencePointWriteDto
            {
                TargetContactId = seed.ContactId,
                Kind = EvidenceKind.Signal,
                Label = "API Evidence",
                SourcePlatform = "Integration test",
                SourceUrl = "https://example.test/evidence",
                Summary = "API evidence created by integration tests.",
                RawSnippet = "Created API evidence snippet.",
                CapturedAtUtc = DateTime.UtcNow,
                ConfidenceScore = 0.82m,
                IsQualificationSignal = true
            },
            seed => new EvidencePointWriteDto
            {
                TargetContactId = seed.ContactId,
                Kind = EvidenceKind.Verification,
                Label = "API Evidence Updated",
                SourcePlatform = "Integration test",
                SourceUrl = "https://example.test/evidence-updated",
                Summary = "API evidence updated by integration tests.",
                RawSnippet = "Updated API evidence snippet.",
                CapturedAtUtc = DateTime.UtcNow,
                ConfidenceScore = 0.92m,
                IsQualificationSignal = false
            }),
        new(
            "lead dossiers",
            "/api/lead-dossiers",
            "API dossier",
            seed => new LeadDossierWriteDto
            {
                MissionRunId = seed.RunId,
                TargetCompanyId = seed.CompanyId,
                TargetContactId = seed.ContactId,
                LeadgenScore = 73,
                SuggestedApproach = "API dossier outreach approach.",
                AdvantagePoint = "API dossier advantage point.",
                IsReadyForOutreach = true,
                CreatedAtUtc = DateTime.UtcNow,
                LastUpdatedAtUtc = DateTime.UtcNow,
                SupportingEvidenceCount = 2
            },
            seed => new LeadDossierWriteDto
            {
                MissionRunId = seed.RunId,
                TargetCompanyId = seed.CompanyId,
                TargetContactId = seed.ContactId,
                LeadgenScore = 88,
                SuggestedApproach = "Updated API dossier outreach approach.",
                AdvantagePoint = "Updated API dossier advantage point.",
                IsReadyForOutreach = true,
                CreatedAtUtc = DateTime.UtcNow,
                LastUpdatedAtUtc = DateTime.UtcNow.AddMinutes(10),
                SupportingEvidenceCount = 3
            }),
        new(
            "mission attachments",
            "/api/mission-attachments",
            "api-test.txt",
            seed => new MissionAttachmentWriteDto
            {
                BusinessDnaMissionId = seed.MissionId,
                FileName = "api-test.txt",
                FilePath = $"/uploads/missions/{seed.MissionId}/api-test.txt",
                ContentType = "text/plain",
                FileSize = 128,
                CreatedAtUtc = DateTime.UtcNow
            },
            seed => new MissionAttachmentWriteDto
            {
                BusinessDnaMissionId = seed.MissionId,
                FileName = "api-test-updated.txt",
                FilePath = $"/uploads/missions/{seed.MissionId}/api-test-updated.txt",
                ContentType = "text/plain",
                FileSize = 256,
                CreatedAtUtc = DateTime.UtcNow
            })
    ];

    public sealed record CrudScenario(
        string Name,
        string Endpoint,
        string Query,
        Func<SeedGraph, object> Create,
        Func<SeedGraph, object> Update);

    public sealed record SeedGraph(Guid MissionId, Guid RunId, Guid AgentId, Guid CompanyId, Guid ContactId);
}
