# LeadGen MVP

LeadGen is an ASP.NET Core MVC lead discovery MVP. It accepts a business profile, generates a structured ICP, runs a bounded lead discovery workflow with real providers, and stores evidence-backed lead dossiers with contacts and notes.

The application is real-provider only. DeepSeek and Tavily credentials are required for AI/search workflows.

## Projects

```text
src/LeadGen.Core              Domain entities, enums, provider contracts, options
src/LeadGen.Infrastructure    EF Core SQLite, DeepSeek/Tavily providers, workflow, search, logging
src/LeadGen.Web               MVC Razor UI and JSON API
src/LeadGen.Mcp               Local stdio/CLI MCP tool server
tests/LeadGen.Tests           JSON API integration tests
tests/LeadGen.PlaywrightTests Browser workflow and mobile smoke tests
```

## Configuration

Create `.env.local` from `.env.example` and set real provider values:

```bash
DEEPSEEK_API_KEY="..."
DEEPSEEK_BASE_URL="https://api.deepseek.com"
DEEPSEEK_MODEL="deepseek-v4-flash"
TAVILY_API_KEY="..."
MAX_SEARCH_QUERIES_PER_RUN=3
MAX_SEARCH_RESULTS_PER_QUERY=3
MAX_EXTRACT_URLS_PER_RUN=8
MAX_LEADS_PER_RUN=5
PROVIDER_TIMEOUT_SECONDS=25
```

`.env.local` and other `.env*` secret files are ignored by git.

## Local Run

Prerequisite: .NET SDK 8.0.x.

```bash
dotnet restore LeadGen.sln
dotnet build LeadGen.sln

set -a
source .env.local
set +a

dotnet run --project src/LeadGen.Web/LeadGen.Web.csproj
```

Open the URL printed by ASP.NET Core. The app creates `src/LeadGen.Web/App_Data/leadgen.db` on first run. No campaign or lead records are seeded.

Health check:

```bash
curl -sS http://localhost:5050/api/health
```

The health payload reports `provider: "Real"` and a `configured` flag showing whether both DeepSeek and Tavily keys are present.

## Lead Workflow

1. Open `/Campaigns/Create`.
2. Enter the business name, optional website, business location, and what the business does.
3. Save the campaign.
4. Click `Find Leads` from campaign details. The default request is 5 leads.
5. DeepSeek automatically infers the likely buyer categories, industries, pains, buying signals, and search phrases from that business profile before search starts.
6. Review the run details page while it runs. The discovery graph updates from the base campaign node into query, site, page, lead, and contact nodes.
7. Open the generated lead dossiers from the lead list.

The workflow uses DeepSeek for ICP/query planning and Tavily for public web search/extract. Contact discovery uses public extracted pages only. The app does not send outreach email.

## Tests

```bash
dotnet test LeadGen.sln
```

Targeted runs:

```bash
dotnet test tests/LeadGen.Tests/LeadGen.Tests.csproj
dotnet test tests/LeadGen.PlaywrightTests/LeadGen.PlaywrightTests.csproj
```

CRUD, search, logging, error, and mobile smoke tests run without provider calls. Tests that call DeepSeek/Tavily are marked as real-provider tests and run only when `DEEPSEEK_API_KEY` and `TAVILY_API_KEY` are set in the test process environment.

## JSON API

Main endpoints:

- `GET /api/health`
- `GET/POST/PUT/DELETE /api/campaigns`
- `POST /api/campaigns/{id}/generate-icp`
- `POST /api/campaigns/{id}/runs`
- `GET /api/runs/{id}`
- `GET /api/leads?campaignId={id}`
- `GET/POST/PUT/DELETE /api/leads`
- `POST /api/leads/{id}/contacts`
- `PUT/DELETE /api/contacts/{id}`
- `POST /api/leads/{id}/notes`
- `PUT/DELETE /api/notes/{id}`
- `GET /api/search?q=term`
- `GET /api/logs?take=100`

Errors return:

```json
{
  "error": {
    "code": "string",
    "message": "safe user-readable message",
    "correlationId": "string"
  }
}
```

## MCP

Run the local MCP server with the same environment variables as the web app:

```bash
set -a
source .env.local
set +a
dotnet run --project src/LeadGen.Mcp
```

Direct CLI checks:

```bash
dotnet run --project src/LeadGen.Mcp -- --tool leadgen_health
dotnet run --project src/LeadGen.Mcp -- --tool list_campaigns
dotnet run --project src/LeadGen.Mcp -- --tool search_leads --query CRM
```

IDE config example:

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

Tools exposed:

`leadgen_health`, `list_campaigns`, `get_campaign`, `create_campaign`, `start_lead_run`, `get_run`, `search_leads`, `get_lead_dossier`, `update_lead_status`, `add_lead_note`.

## Azure App Service Deploy

The default deploy target is Azure App Service `F1 Free`. This is enough for a school/demo URL and avoids hosting charges, but it has limited CPU/storage and no production SLA. Use a paid tier such as `B1` only if the free tier is too slow for the demo.

```bash
dotnet publish src/LeadGen.Web/LeadGen.Web.csproj -c Release -o publish
az group create -n leadgen-rg -l westeurope
az appservice plan create -g leadgen-rg -n leadgen-plan --sku F1 --is-linux
az webapp create -g leadgen-rg -p leadgen-plan -n <unique-app-name> --runtime "DOTNETCORE:8.0"
az webapp config appsettings set -g leadgen-rg -n <unique-app-name> --settings \
  DEEPSEEK_API_KEY="<key>" \
  DEEPSEEK_BASE_URL="https://api.deepseek.com" \
  DEEPSEEK_MODEL="deepseek-v4-flash" \
  TAVILY_API_KEY="<key>" \
  MAX_SEARCH_QUERIES_PER_RUN=3 \
  MAX_SEARCH_RESULTS_PER_QUERY=3 \
  MAX_EXTRACT_URLS_PER_RUN=8 \
  MAX_LEADS_PER_RUN=5 \
  PROVIDER_TIMEOUT_SECONDS=25
az webapp deploy -g leadgen-rg -n <unique-app-name> --src-path publish
```

The app will be available at `https://<unique-app-name>.azurewebsites.net`. For persistent SQLite storage on App Service, configure `ConnectionStrings__DefaultConnection` to a writable persistent path.

Optional paid reliability upgrade:

```bash
az appservice plan update -g leadgen-rg -n leadgen-plan --sku B1
```

## Responsible Use

- No automatic email sending.
- No LinkedIn scraping.
- No login-gated scraping.
- No guessed personal emails.
- Public evidence and contact options are stored for manual review only.
