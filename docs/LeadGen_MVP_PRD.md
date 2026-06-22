# LeadGen MVP PRD — Low-Cost AI Client Discovery Platform

**Document status:** Implementation-ready MVP PRD  
**Target implementation:** ASP.NET Core MVC / .NET, EF Core, Bootstrap, SQLite by default  
**Primary goal:** Build a cheap, reliable school-demo MVP that accepts a business description, generates an ideal-customer profile, searches the web for likely client companies, scores them, and creates a lead dossier with evidence, reasons, and contact options.

---

## 1. Executive decision

Build **one .NET MVC monolith** with a deterministic “agentic workflow,” not a complex autonomous swarm. The workflow should feel agentic to the user, but be simple enough to build, test, and demo without crashes:

1. User enters business details.
2. AI converts the text into a structured Ideal Customer Profile, or ICP.
3. A lead discovery run creates search queries from the ICP.
4. The app searches the public web through Tavily.
5. The app extracts candidate companies and deduplicates them by domain.
6. AI scores each company and writes a small dossier.
7. The app stores leads, contacts, source URLs, notes, and run logs.
8. The UI supports CRUD, global search, responsive views, API tests, Playwright E2E, cloud deploy, logging, and MCP tools.

**Default low-cost stack:**

| Area | MVP choice | Why |
|---|---|---|
| Web app | ASP.NET Core MVC | Matches current development plan and supports Razor views, controllers, APIs, tests, and easy Azure deployment. |
| Data | SQLite + EF Core | Zero database cost, easiest local/cloud demo. Optional Azure SQL free tier later. |
| LLM | DeepSeek `deepseek-v4-flash` | Very low token cost, OpenAI-compatible API format, supports JSON output and tool calls. |
| Web search/extract | Tavily basic search + extract | Free monthly credits and agent-friendly results. Avoid scraping from Google manually. |
| Contact enrichment | Public website emails/contact pages by default; Hunter optional | Keeps MVP cheap. Hunter is useful only when verified emails matter. |
| Agent orchestration | Simple C# service pipeline | Cheaper and easier than full swarm framework. Can migrate to Microsoft Agent Framework later. |
| MCP | Official MCP C# SDK, local stdio server project | Satisfies agentic IDE access without making the web app more fragile. |
| UI | Razor views + Bootstrap 5 | Fast, responsive, no SPA complexity. |
| Tests | xUnit/WebApplicationFactory + Playwright .NET | Covers API endpoints and required 10-step browser scenario. |
| Logging | ASP.NET `ILogger` + Serilog rolling file | Satisfies logging criterion and helps demo/debug. |
| Cloud | Azure App Service Free F1 or Basic B1 for demo-safe mode | Free if possible; B1 only if F1 limits hurt demo. |

---

## 2. Project criteria coverage

The MVP must intentionally cover the course/project criteria.

| Criterion | MVP implementation |
|---|---|
| Cloud deploy | Deploy to Azure App Service. F1 free for lowest cost; B1 only for more reliable demo. Include `/health`. |
| API tests | Integration tests for every JSON API endpoint using xUnit + WebApplicationFactory + test database. |
| Playwright 10-step scenario | One E2E flow from campaign creation to lead dossier and note update. |
| AI integration | “Generate ICP from prompt” and “Find leads” both use AI when API key exists. Fake provider is used in tests. |
| Global search | Header search and `/Search` page across menu items, campaigns, leads, contacts, notes, and dossier text. |
| Logging | Rolling file logs, request correlation ID, AI/search provider logs, run-level errors. |
| Responsive UI | Bootstrap layout with mobile navbar, responsive cards/tables, and no horizontal overflow on 390px width. |
| CRUD stability | Campaigns, leads, contacts, notes, and run records support stable create/read/update/delete where appropriate. |
| MCP exposure | Local `LeadGen.Mcp` server exposes tools callable from Codex/Cursor/other agentic IDE. |
| Functional impression | Demo mode, seed data, provider fallbacks, timeouts, friendly error pages, and no crashes during missing API keys. |
| Oral code understanding | Clear service boundaries, readable names, docs, comments only where helpful, and simple architecture. |

---

## 3. Problem statement

Small businesses often know their product or service but do not know who to contact. Manual lead research is slow: searching for businesses, checking whether they match the offer, finding a useful reason to contact them, and locating a contact email or contact page can take many minutes per lead.

The MVP solves this by generating **researched lead dossiers**:

- Company name and domain.
- Why this company appears to be a fit.
- Evidence from public source URLs.
- Suggested contact angle.
- Public contact options such as company email, contact page, or generic business contact.
- Confidence score and fit score.
- Editable status, notes, and contacts.

The MVP does **not** send outreach emails automatically. It only helps users research and organize leads.

---

## 4. Goals and non-goals

### 4.1 Goals

1. Let a user describe their business in natural language.
2. Convert that description into a structured ICP.
3. Start a lead discovery run from a campaign.
4. Find at least 5 to 10 plausible lead companies per run.
5. Show each lead as a dossier with reasons, evidence URLs, and contact options.
6. Keep monthly cost near zero for school/demo usage.
7. Pass automated API tests and one Playwright scenario.
8. Deploy to cloud and remain usable even when paid API keys are missing.
9. Expose MCP tools for agentic IDE access.

### 4.2 Non-goals for MVP

1. No automatic cold-email sending.
2. No CRM integrations such as HubSpot or Salesforce.
3. No LinkedIn scraping or login-gated data scraping.
4. No complex multi-tenant billing.
5. No full autonomous swarm that keeps browsing indefinitely.
6. No paid contact-data dependency required for demo.
7. No machine-learning training or fine-tuning.

---

## 5. Users and personas

### 5.1 Primary user: small business owner or student demo user

The primary user has a business idea or service and wants a list of possible clients.

Example: “I build websites for local dental clinics in Croatia.”

Needs:

- Easy form.
- AI help structuring the business description.
- Leads with clear reasons, not just a raw list.
- Contact options.
- Ability to edit/delete leads and add notes.

### 5.2 Secondary user: evaluator / professor / code reviewer

The evaluator needs to see that the application is functional, understandable, and satisfies the required technical criteria.

Needs:

- Cloud URL.
- Tests.
- AI integration.
- Global search.
- Logs.
- Responsive UI.
- Working CRUD.
- MCP server callable from an agentic IDE.
- Code that the developer can explain orally.

---

## 6. Core user flow

1. User opens the dashboard.
2. User creates a new campaign.
3. User enters:
   - Business name.
   - Website, optional.
   - What they sell.
   - Target geography.
   - Target customer type.
   - Exclusions.
   - Number of leads to find, default 10.
4. User clicks **Generate ICP**.
5. AI fills structured ICP fields:
   - Target industries.
   - Search keywords.
   - Pain points.
   - Ideal company signals.
   - Negative filters.
6. User saves the campaign.
7. User clicks **Find Leads**.
8. The run status changes: Queued → Running → Completed or Failed.
9. The app displays discovered leads sorted by fit score.
10. User opens a lead dossier.
11. User sees:
    - Company summary.
    - Fit score.
    - Reasons to contact.
    - Evidence URLs.
    - Contact page/email if found.
    - Suggested first outreach angle.
12. User updates lead status or adds a note.
13. User uses global search to find a lead, campaign, contact, note, or menu item.

---

## 7. System architecture

### 7.1 Simple MVP architecture

```text
Browser
  |
  v
ASP.NET Core MVC App
  |-- Razor Views + Bootstrap
  |-- MVC Controllers
  |-- JSON API Controllers
  |-- Application Services
  |-- Agentic LeadDiscoveryWorkflow
  |-- EF Core DbContext
  |-- Provider interfaces
        |-- IAiClient: DeepSeek real / Fake test
        |-- IWebSearchClient: Tavily real / Fake test
        |-- IContactEnrichmentClient: Public web / Hunter optional / Fake test
  |
  v
SQLite database file

Separate local project:
LeadGen.Mcp
  |-- MCP tools call same app services or HTTP API
```

### 7.2 Why this is cheaper and safer than a full swarm

A real “agent swarm” can create unpredictable token spend, slow demos, and hard-to-debug failures. The MVP should use a **bounded workflow** with agent-like steps. Each step has a maximum number of calls, maximum tokens, timeout, and logged output.

The code should still be organized as if each step is an agent:

- `ProfileInterpreterAgent`
- `SearchPlannerAgent`
- `WebResearchAgent`
- `CandidateExtractorAgent`
- `FitScoringAgent`
- `ContactFinderAgent`
- `DossierWriterAgent`

These can later be moved into Microsoft Agent Framework if needed.

---

## 8. Technology stack

### 8.1 Application

- **ASP.NET Core MVC** targeting `net8.0` for broad compatibility.
- **Razor views** for pages.
- **Controllers** for HTML and JSON APIs.
- **Bootstrap 5** for responsive design.
- **EF Core** with SQLite provider by default.
- **xUnit** for unit/integration tests.
- **Playwright .NET** for E2E browser tests.

### 8.2 AI provider

Default real provider:

- Base URL: `https://api.deepseek.com`
- Model: `deepseek-v4-flash`
- Mode: non-thinking by default for cheapest predictable output.
- Output: JSON whenever possible.
- HTTP: call OpenAI-compatible chat completion endpoint using `HttpClient`.

Provider abstraction:

```csharp
public interface IAiClient
{
    Task<T> GenerateJsonAsync<T>(AiRequest request, CancellationToken ct);
    Task<string> GenerateTextAsync(AiRequest request, CancellationToken ct);
}
```

Implementations:

- `DeepSeekAiClient`
- `FakeAiClient` for tests/demo fallback

Optional later:

- `GeminiAiClient`
- `OpenAiClient`

### 8.3 Web search provider

Default:

- Tavily Basic Search for query results.
- Tavily Basic Extract for selected URLs.

Provider abstraction:

```csharp
public interface IWebSearchClient
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(string query, int maxResults, CancellationToken ct);
    Task<IReadOnlyList<ExtractedPageDto>> ExtractAsync(IEnumerable<string> urls, CancellationToken ct);
}
```

Implementations:

- `TavilySearchClient`
- `FakeSearchClient`

### 8.4 Contact enrichment

Default cheap strategy:

1. Use search/extract result snippets.
2. Check company website home/contact/about pages if already discovered.
3. Extract only public generic contact emails, phone numbers, and contact page URLs.
4. Store confidence and source URL.

Optional Hunter strategy if `HUNTER_API_KEY` exists:

- Domain Search for company domain.
- Email Finder only if a person name is known.
- Email Verifier only for candidate emails.

### 8.5 Database

Default local/cloud demo:

- SQLite file: `App_Data/leadgen.db`
- EF Core migrations.

Optional cloud database:

- Azure SQL free tier if SQLite persistence is not acceptable.

### 8.6 MCP

Create a separate project:

```text
src/LeadGen.Mcp/LeadGen.Mcp.csproj
```

Use the official MCP C# SDK.

MCP transport for MVP:

- Local stdio server.
- Agentic IDE command example:

```json
{
  "mcpServers": {
    "leadgen": {
      "command": "dotnet",
      "args": ["run", "--project", "src/LeadGen.Mcp"]
    }
  }
}
```

---

## 9. Data model

### 9.1 Entities

#### Campaign

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| Name | string | Required |
| BusinessName | string | Required |
| WebsiteUrl | string? | Optional |
| BusinessDescription | string | Required |
| TargetGeography | string? | Optional |
| TargetCustomers | string? | Optional |
| Exclusions | string? | Optional |
| IcpJson | string? | AI-generated structured ICP |
| CreatedAtUtc | DateTime | Required |
| UpdatedAtUtc | DateTime | Required |

#### LeadSearchRun

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| CampaignId | Guid | Required |
| Status | enum | Queued, Running, Completed, Failed, Cancelled |
| RequestedLeadCount | int | Default 10, max 25 for MVP |
| SearchQueriesJson | string? | Generated queries |
| StartedAtUtc | DateTime? | Optional |
| CompletedAtUtc | DateTime? | Optional |
| ErrorMessage | string? | Safe error text |
| EstimatedCostUsd | decimal | Calculated estimate |
| LogsJson | string? | Step summaries, not secrets |

#### Lead

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| CampaignId | Guid | Required |
| LeadSearchRunId | Guid? | Optional |
| CompanyName | string | Required |
| Domain | string? | Dedup key |
| WebsiteUrl | string? | Optional |
| Industry | string? | Optional |
| Location | string? | Optional |
| FitScore | int | 0 to 100 |
| ConfidenceScore | int | 0 to 100 |
| Status | enum | New, Reviewed, Contacted, Qualified, Rejected |
| MatchReasonsJson | string | Array of reasons |
| EvidenceJson | string | Array of `{title,url,quoteOrSummary}` |
| DossierMarkdown | string | AI-generated dossier |
| SuggestedOutreachAngle | string? | Optional |
| CreatedAtUtc | DateTime | Required |
| UpdatedAtUtc | DateTime | Required |

#### LeadContact

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| LeadId | Guid | Required |
| Type | enum | Email, ContactPage, Phone, Social, Other |
| Value | string | Required |
| SourceUrl | string? | Required when possible |
| ConfidenceScore | int | 0 to 100 |
| IsVerified | bool | Hunter or syntax verification |
| CreatedAtUtc | DateTime | Required |

#### LeadNote

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| LeadId | Guid | Required |
| Body | string | Required |
| CreatedAtUtc | DateTime | Required |
| UpdatedAtUtc | DateTime | Required |

#### AiCallLog

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| Purpose | string | `GenerateIcp`, `PlanQueries`, etc. |
| Provider | string | DeepSeek/Fake |
| Model | string | `deepseek-v4-flash` |
| InputTokens | int? | If returned by provider |
| OutputTokens | int? | If returned by provider |
| EstimatedCostUsd | decimal | Calculated estimate |
| DurationMs | int | Required |
| Success | bool | Required |
| ErrorMessage | string? | No secrets |
| CreatedAtUtc | DateTime | Required |

---

## 10. Workflow details

### 10.1 Generate ICP

Input:

- Business description.
- Business website if provided.
- Target geography.
- Target customers.
- Exclusions.

Output JSON:

```json
{
  "summary": "string",
  "targetIndustries": ["string"],
  "targetLocations": ["string"],
  "buyerTypes": ["string"],
  "painPoints": ["string"],
  "positiveSignals": ["string"],
  "negativeSignals": ["string"],
  "searchKeywords": ["string"],
  "exampleQueries": ["string"]
}
```

Rules:

- Never generate more than 12 search keywords.
- Keep output language English unless user asks otherwise.
- Store full JSON in `Campaign.IcpJson`.
- Let user edit and save the campaign after generation.

### 10.2 Plan search queries

Input:

- Campaign fields.
- ICP JSON.
- Requested lead count.

Output JSON:

```json
{
  "queries": [
    {
      "query": "string",
      "purpose": "string"
    }
  ]
}
```

Rules:

- Generate 3 to 5 queries.
- Use local/geographic modifiers when present.
- Avoid extremely broad queries.
- Avoid LinkedIn-only queries.
- Do not search for private personal data.

### 10.3 Search and extract

For each generated query:

- Call Tavily basic search with max 5 results.
- Store title, URL, snippet, and domain.
- Deduplicate by normalized domain.
- Select up to 20 URLs for extraction.
- Clip extracted text to a safe token budget before sending to AI.

### 10.4 Candidate extraction

AI receives batches of search/extract summaries and returns candidate companies.

Output JSON:

```json
{
  "candidates": [
    {
      "companyName": "string",
      "domain": "string",
      "websiteUrl": "string",
      "industry": "string",
      "location": "string",
      "evidence": [
        {
          "url": "string",
          "title": "string",
          "summary": "string"
        }
      ],
      "rawReasons": ["string"]
    }
  ]
}
```

Rules:

- Only include organizations that appear to be businesses/clients, not directories or blog posts.
- Prefer company homepages, service pages, directories with clear company URLs, or public business profiles.
- Deduplicate by domain.

### 10.5 Fit scoring and dossier

Each lead gets:

- FitScore 0 to 100.
- ConfidenceScore 0 to 100.
- 3 to 5 match reasons.
- 1 suggested outreach angle.
- Dossier markdown of 150 to 300 words.

Scoring guideline:

| Score | Meaning |
|---|---|
| 90-100 | Very strong ICP match, multiple evidence signals, good contact path. |
| 75-89 | Good match, at least two strong signals. |
| 60-74 | Possible match, limited evidence. |
| 40-59 | Weak match, keep only if lead quota not reached. |
| 0-39 | Reject unless user explicitly includes it. |

### 10.6 Contact discovery

Default cheap public-contact logic:

1. If extracted text contains public emails, store them as contacts.
2. If no email, store contact page URL if discovered.
3. If no contact page, store website URL as `Other` with low confidence.

Rules:

- Do not guess personal emails.
- Do not scrape LinkedIn or login-gated websites.
- Do not store sensitive personal data.
- Add source URL for every contact when possible.

---

## 11. UI requirements

### 11.1 Main pages

| Page | URL | Purpose |
|---|---|---|
| Dashboard | `/` | Show recent campaigns, recent runs, CTA to create campaign. |
| Campaign list | `/Campaigns` | CRUD list. |
| Campaign create/edit | `/Campaigns/Create`, `/Campaigns/Edit/{id}` | Business profile form + Generate ICP button. |
| Campaign details | `/Campaigns/Details/{id}` | ICP, run button, run history, leads summary. |
| Run details | `/Runs/Details/{id}` | Status, progress, generated search queries, errors, created leads. |
| Lead list | `/Leads?campaignId={id}` | Filterable leads table/cards. |
| Lead dossier | `/Leads/Details/{id}` | Dossier, evidence, contacts, status, notes. |
| Global search | `/Search?q={term}` | Search menu/data. |
| Logs viewer | `/Admin/Logs` | Last 200 safe log lines for demo/debug. Can be disabled in production. |
| About/Help | `/Home/About` | Explain demo and responsible-use boundaries. |

### 11.2 Responsive requirements

- Mobile navbar collapses at Bootstrap `lg` breakpoint or lower.
- Campaign/lead tables become stacked cards or horizontally scroll safely on small screens.
- Lead dossier is readable on 390px width.
- Buttons have visible loading states.
- No horizontal page overflow on 390px width.

---

## 12. API requirements

All JSON APIs must return consistent error objects:

```json
{
  "error": {
    "code": "string",
    "message": "safe user-readable message",
    "correlationId": "string"
  }
}
```

### 12.1 Endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/health` | Health check. |
| GET | `/api/campaigns` | List campaigns. |
| POST | `/api/campaigns` | Create campaign. |
| GET | `/api/campaigns/{id}` | Get campaign. |
| PUT | `/api/campaigns/{id}` | Update campaign. |
| DELETE | `/api/campaigns/{id}` | Delete campaign and related records. |
| POST | `/api/campaigns/{id}/generate-icp` | Generate ICP with AI. |
| POST | `/api/campaigns/{id}/runs` | Start lead discovery run. |
| GET | `/api/runs/{id}` | Get run status/details. |
| GET | `/api/leads?campaignId={id}` | List leads. |
| GET | `/api/leads/{id}` | Get lead dossier data. |
| PUT | `/api/leads/{id}` | Update lead status/core editable fields. |
| DELETE | `/api/leads/{id}` | Delete lead. |
| POST | `/api/leads/{id}/contacts` | Add contact. |
| PUT | `/api/contacts/{id}` | Update contact. |
| DELETE | `/api/contacts/{id}` | Delete contact. |
| POST | `/api/leads/{id}/notes` | Add note. |
| PUT | `/api/notes/{id}` | Update note. |
| DELETE | `/api/notes/{id}` | Delete note. |
| GET | `/api/search?q={term}` | Global search. |
| GET | `/api/logs?take=100` | Safe log tail for demo. |

---

## 13. MCP requirements

### 13.1 MCP tools

The MCP server should expose these tools:

| Tool | Input | Output | Purpose |
|---|---|---|---|
| `leadgen_health` | none | status string | Prove MCP connection works. |
| `list_campaigns` | optional search text | campaign summaries | Let IDE inspect data. |
| `get_campaign` | campaign id | campaign details | Read campaign and ICP. |
| `create_campaign` | campaign fields | campaign id | Create campaign from IDE. |
| `start_lead_run` | campaign id, lead count | run id/status | Start discovery from IDE. |
| `get_run` | run id | status/logs/lead count | Inspect run. |
| `search_leads` | query text | lead summaries | Search lead database. |
| `get_lead_dossier` | lead id | lead dossier markdown/contact/evidence | Read complete dossier. |
| `update_lead_status` | lead id, status | updated status | Change lead state. |
| `add_lead_note` | lead id, note body | note id | Add note from IDE. |

### 13.2 MCP acceptance

- Running `dotnet run --project src/LeadGen.Mcp` starts the MCP server without throwing.
- The `leadgen_health` tool returns `ok`.
- At least three tools can be called from an agentic IDE during demo: `list_campaigns`, `search_leads`, `get_lead_dossier`.
- The MCP server must use the same database connection string as the web app.
- MCP is local-dev only by default; do not expose it publicly without authentication.

---

## 14. Configuration

Use environment variables or user secrets. Never commit real keys.

```text
ConnectionStrings__DefaultConnection=Data Source=App_Data/leadgen.db
AI_PROVIDER=DeepSeek
DEEPSEEK_API_KEY=
DEEPSEEK_BASE_URL=https://api.deepseek.com
DEEPSEEK_MODEL=deepseek-v4-flash
TAVILY_API_KEY=
HUNTER_API_KEY=
USE_FAKE_PROVIDERS=false
ENABLE_MCP=true
ENABLE_ADMIN_LOG_VIEWER=true
MAX_SEARCH_QUERIES_PER_RUN=5
MAX_SEARCH_RESULTS_PER_QUERY=5
MAX_EXTRACT_URLS_PER_RUN=20
MAX_LEADS_PER_RUN=10
MAX_CONCURRENT_PROVIDER_CALLS=2
MAX_RUN_ESTIMATED_COST_USD=0.25
PROVIDER_TIMEOUT_SECONDS=25
```

### 14.1 Demo fallback behavior

If keys are missing:

- `USE_FAKE_PROVIDERS=true` should show deterministic sample AI/search results.
- The app must display a warning banner: “Demo provider active — results are sample data.”
- Tests must always use fake providers.
- The cloud demo should not crash if a key is missing.

---

## 15. Acceptance criteria

These acceptance criteria are written so Codex `/goal` can implement and verify them.

### AC-0 — Repository, build, and run

**Given** a fresh clone of the repository,  
**when** a developer runs `dotnet restore`, `dotnet build`, and `dotnet test`,  
**then** the solution builds and all tests pass without requiring external API keys.

Requirements:

- Solution file: `LeadGen.sln`.
- Projects:
  - `src/LeadGen.Web`
  - `src/LeadGen.Core`
  - `src/LeadGen.Infrastructure`
  - `src/LeadGen.Mcp`
  - `tests/LeadGen.Tests`
  - `tests/LeadGen.PlaywrightTests`
- `README.md` includes local run, test, and deploy instructions.
- `docs/LeadGen_MVP_PRD.md` contains this PRD.

### AC-1 — Campaign CRUD

**Given** the user opens `/Campaigns`,  
**when** they create, edit, view, and delete a campaign,  
**then** each operation succeeds and the UI shows validation errors for invalid input.

Required fields:

- Campaign name.
- Business name.
- Business description.

Pass/fail checks:

- Empty required fields show validation messages.
- Edit persists updated data.
- Delete removes campaign and related leads/runs/notes/contacts.
- API tests cover create/read/update/delete.

### AC-2 — AI-generated ICP

**Given** a saved or unsaved campaign form with a business description,  
**when** the user clicks **Generate ICP**,  
**then** the app calls `IAiClient`, receives structured JSON, displays it, and allows saving it.

Pass/fail checks:

- With `USE_FAKE_PROVIDERS=true`, deterministic ICP JSON is returned.
- With real `DEEPSEEK_API_KEY`, the app calls DeepSeek and logs an `AiCallLog` row.
- The UI handles provider failure with a friendly error and does not lose form data.
- Generated JSON is stored in `Campaign.IcpJson`.
- API test covers `/api/campaigns/{id}/generate-icp` with fake provider.

### AC-3 — Start lead discovery run

**Given** a campaign with business description and ICP,  
**when** the user clicks **Find Leads**,  
**then** a `LeadSearchRun` is created and transitions through valid statuses.

Pass/fail checks:

- Default requested lead count is 10.
- Max lead count is enforced by configuration.
- Run status is visible on `/Runs/Details/{id}`.
- Provider timeouts are handled as Failed status, not app crashes.
- The run logs generated search queries.
- API test covers `POST /api/campaigns/{id}/runs` and `GET /api/runs/{id}`.

### AC-4 — Lead generation output

**Given** a completed fake-provider lead discovery run,  
**when** the user opens the campaign details or lead list,  
**then** at least 5 leads are displayed with fit score, reasons, evidence, and contact option.

Pass/fail checks:

- Duplicate domains are not inserted twice in the same campaign.
- Leads are sorted by `FitScore` descending.
- Each lead has at least one evidence URL or source note.
- Each lead has a `DossierMarkdown` field.
- Each lead has a status defaulting to `New`.

### AC-5 — Lead dossier page

**Given** a generated lead,  
**when** the user opens `/Leads/Details/{id}`,  
**then** the page shows the company, fit score, match reasons, evidence, contacts, suggested outreach angle, status, and notes.

Pass/fail checks:

- User can update status.
- User can add and delete a note.
- User can add/edit/delete a contact.
- Markdown dossier is rendered safely.
- Missing contact info shows “No public contact found yet” instead of crashing.

### AC-6 — Global search

**Given** campaigns, leads, notes, contacts, and menu items exist,  
**when** the user searches from the header or `/Search?q=...`,  
**then** the app returns grouped results for menu pages and data records.

Search targets:

- Menu/page names.
- Campaign name/business description/ICP JSON.
- Lead company/domain/dossier/reasons.
- Contact values.
- Note body.

Pass/fail checks:

- `/api/search?q=...` returns JSON results.
- `/Search?q=...` renders results grouped by type.
- Empty query shows validation/help, not all data.
- Search works on mobile navbar.

### AC-7 — Logging

**Given** the app is running,  
**when** users create campaigns, generate ICP, start runs, and encounter provider errors,  
**then** structured logs are written and can be inspected.

Pass/fail checks:

- Logs include correlation ID.
- Logs include run id/campaign id where relevant.
- Logs never include API keys.
- File logs roll daily or by size.
- `/Admin/Logs` displays the last safe log lines when enabled.
- At least one integration test verifies error response includes correlation ID.

### AC-8 — API tests for all endpoints

**Given** the test project is executed,  
**when** `dotnet test` runs,  
**then** every API endpoint has at least one success-path test and key invalid-input tests.

Minimum test list:

- `Health_ReturnsOk`
- `Campaigns_Crud_Works`
- `GenerateIcp_WithFakeProvider_ReturnsJson`
- `StartRun_WithFakeProvider_CreatesLeads`
- `RunStatus_ReturnsCompleted`
- `Leads_Crud_Works`
- `Contacts_Crud_Works`
- `Notes_Crud_Works`
- `GlobalSearch_ReturnsMenuAndDataResults`
- `Errors_IncludeCorrelationId`

Tests must use an isolated SQLite database and fake providers.

### AC-9 — Playwright 10-step E2E scenario

Create one Playwright .NET browser test named `LeadGenHappyPath_10Steps`.

Steps:

1. Open home page.
2. Navigate to Campaigns.
3. Create a new campaign.
4. Click Generate ICP.
5. Save campaign.
6. Start lead discovery run.
7. Wait for completed status.
8. Open the generated lead list.
9. Use global search to find one lead.
10. Open lead dossier and add a note.

Extra optional steps:

11. Update lead status to Reviewed.
12. Add a contact page contact.
13. Return to dashboard and verify recent run/lead appears.

Pass/fail checks:

- Test runs using fake providers.
- Test does not require external internet.
- Test asserts visible text on each major page.

### AC-10 — Responsive UI

**Given** the app is viewed on desktop and mobile viewport,  
**when** the main pages are opened,  
**then** navigation and content remain usable.

Pass/fail checks:

- Playwright mobile viewport test at 390x844 opens dashboard, campaign form, lead list, and lead dossier.
- Navbar collapses and search is reachable.
- No horizontal overflow is detected on key pages.

### AC-11 — MCP server

**Given** the database contains at least one campaign and lead,  
**when** an agentic IDE starts `dotnet run --project src/LeadGen.Mcp`,  
**then** MCP tools can inspect and update LeadGen data.

Pass/fail checks:

- `leadgen_health` returns `ok`.
- `list_campaigns` returns at least seed/demo campaign.
- `search_leads` returns matching leads.
- `get_lead_dossier` returns dossier markdown.
- `add_lead_note` creates a note visible in the web UI.
- MCP does not expose secrets.

### AC-12 — Cloud deploy

**Given** the app is deployed to Azure App Service,  
**when** the evaluator opens the cloud URL,  
**then** the app loads, `/api/health` returns OK, and demo mode works.

Pass/fail checks:

- `README.md` includes Azure App Service deploy instructions.
- App creates `App_Data` directory if missing.
- SQLite database is initialized/migrated on startup in Development/Demo mode.
- App settings can configure real API keys.
- Missing keys do not crash the app.

### AC-13 — Cost guardrails

**Given** a user starts a lead run,  
**when** the planned query/result/token count exceeds configured limits,  
**then** the app refuses or trims the run and explains why.

Pass/fail checks:

- Max 5 queries per run.
- Max 5 search results per query.
- Max 20 extracted URLs per run.
- Max 10 leads by default, configurable to 25 max for MVP.
- Max 2 concurrent provider calls.
- Estimated cost is shown before or after each run.
- Run stops if estimated cost exceeds `MAX_RUN_ESTIMATED_COST_USD`.

---

## 16. Testing strategy

### 16.1 Unit tests

Test pure services:

- Domain normalization.
- Deduplication.
- Score parsing.
- Cost estimator.
- Global search ranking.
- Contact extractor regex and contact-page detection.

### 16.2 Integration tests

Use `WebApplicationFactory` with test configuration:

```text
USE_FAKE_PROVIDERS=true
ConnectionStrings__DefaultConnection=Data Source=:memory:
```

Test all API endpoints.

### 16.3 E2E tests

Use Playwright .NET.

- One desktop happy path.
- One mobile smoke test.

### 16.4 Manual demo checklist

1. Open cloud URL.
2. Show responsive mobile view.
3. Create campaign from prompt.
4. Generate ICP.
5. Find leads.
6. Open dossier.
7. Add note and status.
8. Use global search.
9. Show logs.
10. Show MCP call from IDE.
11. Show tests passing.

---

## 17. Deployment plan

### 17.1 Cheapest deploy

Use Azure App Service F1 Free with SQLite.

Pros:

- $0 hosting.
- Meets cloud deploy criterion.
- Good for learning/demo.

Cons:

- 60 CPU minutes/day.
- No SLA.
- Limited storage.
- May sleep or throttle.

### 17.2 Demo-safe deploy

Use Azure App Service Basic B1 Linux with SQLite or Azure SQL free tier.

Pros:

- More reliable for demo.
- More CPU and RAM.

Cons:

- Roughly low double-digit USD/month depending on region.

### 17.3 Optional database upgrade

Use Azure SQL Database free offer when:

- SQLite persistence is not trusted.
- You want a managed database.
- You want easier remote inspection.

For MVP, keep SQLite first because it is simpler and free.

---

## 18. Security, privacy, and responsible use

1. Do not commit API keys.
2. Use user secrets locally and App Service settings in cloud.
3. Do not scrape login-gated sites.
4. Do not scrape LinkedIn.
5. Do not generate or guess personal emails.
6. Prefer generic business contacts and contact pages.
7. Store evidence/source URLs for each lead.
8. Let users delete campaigns/leads/contacts.
9. Show confidence scores and do not claim uncertain info as fact.
10. Add footer disclaimer: “LeadGen uses public web data and AI-generated summaries. Verify before outreach.”

---

## 19. Cost estimate

### 19.1 Current researched pricing inputs

These are the pricing assumptions used for the MVP estimate. Verify before production use because API pricing changes.

- DeepSeek official docs list `deepseek-v4-flash` with 1M context, JSON output/tool calls, cache-miss input at **$0.14 / 1M tokens**, cache-hit input at **$0.0028 / 1M tokens**, and output at **$0.28 / 1M tokens**.
- Tavily docs list **1,000 free API credits/month**, pay-as-you-go at **$0.008/credit**, Basic Search at **1 credit/request**, and Basic Extract at **1 credit per 5 successful URL extractions**.
- Azure App Service F1 Free is intended for trials/learning and has 60 CPU minutes/day, 1 GB RAM, 1 GB storage, and no production SLA.
- Azure SQL free offer gives 100,000 vCore seconds, 32 GB data, and 32 GB backup storage per database per month, up to 10 General Purpose databases per subscription.
- Hunter free plan includes credits, and Hunter counts 1 credit for an email found and 0.5 credit for a verified email. Hunter should stay optional.

### 19.2 Per-run estimate for 10 leads

Assumption:

- 5 Tavily searches.
- 20 URL extracts.
- 10 leads generated.
- About 35k AI input tokens.
- About 10k AI output tokens.

Estimated AI cost on DeepSeek V4 Flash:

```text
Input:  35,000 / 1,000,000 * $0.14 = $0.0049
Output: 10,000 / 1,000,000 * $0.28 = $0.0028
Total AI per run ≈ $0.0077
```

Estimated Tavily cost:

```text
5 basic searches = 5 credits
20 basic extracts = 4 credits
Total = 9 credits
Within free 1,000 credits/month: $0
If paid: 9 * $0.008 = $0.072
```

Estimated Hunter cost:

```text
Disabled by default: $0
If enabled for 10 leads with 1 found email and 1 verification each:
10 email-found credits + 5 verification credits = 15 credits/run
```

### 19.3 Monthly estimate

| Usage | Hosting | Database | AI | Tavily | Hunter | Estimated monthly total |
|---|---:|---:|---:|---:|---:|---:|
| Demo only, fake providers | $0 | $0 | $0 | $0 | $0 | $0 |
| 10 real runs/month, 100 leads, no Hunter | $0 F1 | $0 SQLite | ~$0.08 | $0 within free credits | $0 | ~$0.08 plus any minimum top-up |
| 100 real runs/month, 1,000 leads, no Hunter | $0 F1 or B1 | $0 SQLite | ~$0.77 | ~$0 if within 1,000 Tavily credits? 900 credits = free | $0 | ~$0.77 plus hosting if B1 |
| Demo-safe hosting, 10 real runs/month | ~$13/month B1 estimate | $0 SQLite | ~$0.08 | $0 | $0 | ~$13/month |
| Verified email mode | hosting varies | varies | low | low | likely paid Hunter plan needed | budget separately |

Recommendation:

- Build and test with fake providers.
- Demo real AI with DeepSeek V4 Flash only.
- Use Tavily free credits.
- Do not enable Hunter unless the evaluator specifically asks for verified emails.
- Start on Azure F1. Scale to B1 only for demo reliability.

---

## 20. Implementation milestones

### Milestone 1 — Skeleton and data

- Create solution and projects.
- Add EF Core entities/migrations.
- Add seeded demo data.
- Add layout, dashboard, campaign CRUD.
- Add health endpoint.

Exit criteria:

- `dotnet build` passes.
- Campaign CRUD works.
- `/api/health` returns OK.

### Milestone 2 — AI ICP and fake providers

- Add provider interfaces.
- Add fake AI/search/contact providers.
- Add DeepSeek client.
- Add Generate ICP UI/API.
- Add logging for AI calls.

Exit criteria:

- Generate ICP works in fake mode.
- API test passes.

### Milestone 3 — Lead discovery workflow

- Add run entity/status.
- Add workflow services.
- Add Tavily client.
- Add candidate extraction/scoring/dossier.
- Add lead list/details.

Exit criteria:

- Fake run creates at least 5 leads.
- Real run works with API keys.
- Errors become Failed runs, not crashes.

### Milestone 4 — CRUD, search, logs, responsive

- Add contact/note CRUD.
- Add global search UI/API.
- Add log viewer.
- Fix mobile layout.

Exit criteria:

- All CRUD flows work.
- Search returns menu and data results.
- Mobile smoke test passes.

### Milestone 5 — Tests and MCP

- Add integration tests for APIs.
- Add Playwright 10-step scenario.
- Add MCP server project and tools.
- Add README with MCP IDE config.

Exit criteria:

- `dotnet test` passes.
- MCP health/list/search/dossier tools work.

### Milestone 6 — Cloud deploy

- Add deployment instructions.
- Deploy to Azure App Service.
- Configure fake provider or real keys.
- Validate `/api/health` and demo flow.

Exit criteria:

- Cloud URL works.
- Demo checklist passes.

---

## 21. Codex `/goal` prompt

Paste this into Codex after adding this PRD to the repo as `docs/LeadGen_MVP_PRD.md`.

```text
/goal Build the LeadGen MVP according to docs/LeadGen_MVP_PRD.md.

Context:
- This is a low-cost ASP.NET Core MVC lead generation MVP.
- Use a simple deterministic agentic workflow, not a complex autonomous swarm.
- The app must work with fake providers without external API keys, and must optionally use DeepSeek V4 Flash + Tavily when keys are configured.
- Prioritize passing the acceptance criteria in the PRD over adding extra features.

Implementation requirements:
1. Create/organize the solution with these projects:
   - src/LeadGen.Web
   - src/LeadGen.Core
   - src/LeadGen.Infrastructure
   - src/LeadGen.Mcp
   - tests/LeadGen.Tests
   - tests/LeadGen.PlaywrightTests
2. Implement ASP.NET Core MVC Razor UI with Bootstrap responsive layout.
3. Implement EF Core entities and SQLite persistence for Campaign, LeadSearchRun, Lead, LeadContact, LeadNote, and AiCallLog.
4. Implement Campaign CRUD, Lead CRUD, Contact CRUD, Note CRUD, run details, lead dossier, dashboard, global search, admin log viewer, and health endpoint.
5. Implement provider abstractions: IAiClient, IWebSearchClient, IContactEnrichmentClient.
6. Implement Fake providers for tests/demo. They must generate deterministic ICP, search results, leads, dossiers, contacts, and no network calls.
7. Implement DeepSeekAiClient using HttpClient and model deepseek-v4-flash through OpenAI-compatible API. Use JSON outputs and never log secrets.
8. Implement TavilySearchClient for basic search and extract. Keep strict limits from configuration.
9. Implement LeadDiscoveryWorkflow with bounded steps: Generate queries, search, extract, dedupe, score, contact discovery, dossier save. Handle failures by marking LeadSearchRun as Failed, not by crashing.
10. Implement global search across menu items, campaigns, leads, contacts, notes, and dossier text.
11. Implement file/structured logging with correlation IDs and safe error responses.
12. Implement MCP stdio server in src/LeadGen.Mcp with tools: leadgen_health, list_campaigns, get_campaign, create_campaign, start_lead_run, get_run, search_leads, get_lead_dossier, update_lead_status, add_lead_note.
13. Add integration tests for every JSON API endpoint listed in the PRD using fake providers and isolated SQLite.
14. Add Playwright .NET test LeadGenHappyPath_10Steps plus one mobile smoke test.
15. Add README.md with local run, fake-provider demo, real-provider configuration, Azure App Service deploy, test commands, and MCP IDE config.

Important constraints:
- Do not require real API keys for tests or default local demo.
- Do not implement automatic email sending.
- Do not scrape LinkedIn or login-gated websites.
- Do not store or log API keys.
- Use simple readable C# that can be explained orally.
- Keep maximum run limits configurable and enforce them.

Definition of done:
- dotnet restore passes.
- dotnet build passes.
- dotnet test passes.
- The MVC app starts locally with fake providers.
- The 10-step Playwright happy path passes.
- /api/health returns OK.
- A fake lead run creates at least 5 leads with dossier, reasons, evidence, and contact option.
- Global search works from UI and API.
- MCP health/list/search/dossier tools work locally.
- README contains clear setup/deploy instructions.
```

---

## 22. Useful implementation notes for Codex

### 22.1 Recommended folder structure

```text
src/
  LeadGen.Core/
    Entities/
    Enums/
    Models/
    Services/
    Abstractions/
  LeadGen.Infrastructure/
    Data/
    Providers/Ai/
    Providers/Search/
    Providers/Contacts/
    Logging/
  LeadGen.Web/
    Controllers/
    ApiControllers/
    ViewModels/
    Views/
    wwwroot/
  LeadGen.Mcp/
    Program.cs
    Tools/
tests/
  LeadGen.Tests/
  LeadGen.PlaywrightTests/
docs/
  LeadGen_MVP_PRD.md
```

### 22.2 Suggested services

```text
ICampaignService
ILeadDiscoveryWorkflow
IRunService
ILeadService
IGlobalSearchService
ICostEstimator
IContactExtractor
IDomainNormalizer
IAiClient
IWebSearchClient
IContactEnrichmentClient
```

### 22.3 Error-handling pattern

- Controllers should catch expected service exceptions and return safe errors.
- Unexpected errors should be logged and return 500 with correlation ID.
- Provider errors should be stored in run error messages and logs.
- No provider exception should crash the whole app.

### 22.4 Cost controls

Implement `LeadGenOptions` bound from configuration:

```csharp
public sealed class LeadGenOptions
{
    public int MaxSearchQueriesPerRun { get; set; } = 5;
    public int MaxSearchResultsPerQuery { get; set; } = 5;
    public int MaxExtractUrlsPerRun { get; set; } = 20;
    public int MaxLeadsPerRun { get; set; } = 10;
    public int MaxConcurrentProviderCalls { get; set; } = 2;
    public decimal MaxRunEstimatedCostUsd { get; set; } = 0.25m;
    public int ProviderTimeoutSeconds { get; set; } = 25;
}
```

---

## 23. Source notes

Research checked on 2026-06-19:

- DeepSeek API pricing and model capabilities: https://api-docs.deepseek.com/quick_start/pricing
- Tavily credits and pricing: https://docs.tavily.com/documentation/api-credits
- Gemini Developer API pricing alternative: https://ai.google.dev/gemini-api/docs/pricing
- OpenAI API pricing alternative: https://developers.openai.com/api/docs/pricing
- Azure App Service pricing: https://azure.microsoft.com/en-us/pricing/details/app-service/linux/
- Azure SQL free offer: https://learn.microsoft.com/en-us/azure/azure-sql/database/free-offer
- Hunter pricing and credit rules: https://hunter.io/pricing
- Hunter API overview: https://hunter.io/api
- Microsoft Agent Framework overview: https://learn.microsoft.com/en-us/agent-framework/overview/
- Microsoft.Extensions.AI docs: https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai
- MCP C# SDK: https://github.com/modelcontextprotocol/csharp-sdk
- Playwright .NET docs: https://playwright.dev/dotnet/docs/intro
- ASP.NET Core logging docs: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/
