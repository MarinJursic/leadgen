# Leadgen

Leadgen is an ASP.NET Core MVC application for mission-driven B2B lead research.

Instead of starting from a static spreadsheet of companies, the product starts from a company's Business DNA, turns that input into a structured research mission, runs a specialized swarm over companies and contacts, and outputs evidence-backed lead dossiers.

## What The Product Does

Leadgen is organized around three product phases:

1. Intelligence Gate
   Convert raw product input into a structured mission and capture clarification gaps.
2. Investigative Swarm
   Run specialized research roles over companies, contacts, signals, and evidence.
3. Dossier Output
   Produce scored, evidence-backed lead dossiers that are ready for outreach.

The core domain vocabulary used across the project is:

- `BusinessDnaMission`
- `ClarificationQuestion`
- `MissionRun`
- `MissionAgentAssignment`
- `SwarmAgent`
- `TargetCompany`
- `TargetContact`
- `ContactChannel`
- `EvidencePoint`
- `LeadDossier`

## Lab History

### Lab 1

Lab 1 established the product interpretation and the base domain shape for Leadgen.

What was done:

- researched the Leadgen product documents and mapped the lab rubric to the actual product
- designed a mission-first domain model instead of a generic CRUD model
- defined the object graph for missions, runs, agents, companies, contacts, channels, evidence, and dossiers
- planned seeded sample data, meaningful LINQ queries, and an async simulation path aligned with the future swarm workflow
- recorded AI usage and planning notes in `lab-1/`

What remains in the repo from Lab 1:

- `lab-1/implementation-plan.md`
- `lab-1/ai-agent-log.md`
- `lab-1/agent-log.txt`

Important current-state note:

- the original Lab 1 multi-project delivery referenced in the log (`Leadgen.Model` and `Leadgen.Lab1Runner`) is not part of the current runnable repository anymore
- the domain concepts from that work were folded into the MVC app during Lab 2 and then expanded in Lab 3

### Lab 2

Lab 2 turned the project into a single ASP.NET Core MVC application with full entity browsing and a custom UX direction.

What was done:

- consolidated the repo into one runnable MVC app: `leadgen.csproj`
- moved the Leadgen domain model into the web project under `Domain/`
- kept seeded sample data inside the app through `Data/Seed/`
- used a mock read repository as the Lab 2 data source
- added `Index` and `Details` coverage for the full entity graph
- built a custom landing page and mission canvas instead of keeping the default template home page
- added navigation, breadcrumbs, and a dossier-style visual direction
- captured UX planning and sub-agent evidence in `lab-2/`

What remains in the repo from Lab 2:

- the single-project MVC structure
- the domain entities and enums
- the seed factory and query services
- the landing page, mission page, entity controllers, and the overall dossier-style UI
- `lab-2/implementation-plan.md`
- `lab-2/stitch-workflow.md`
- `lab-2/ux-agent-prompt.md`
- `lab-2/ux-sub-agent-log.md`
- `lab-2/hook-capture/`

Important current-state note:

- Lab 2 provided the application shell that still exists today
- its original mock-repository data source has been replaced by EF Core in Lab 3

### Lab 3

Lab 3 converted the Lab 2 MVC app from a read-only mock graph into an EF Core-backed application with custom routing and a real write flow.

What was done:

- made the model EF-ready using data annotations, foreign keys, `virtual` navigation properties, and `ICollection<>`
- added EF Core packages and configured SQLite
- added `Data/LeadgenDbContext.cs`
- added startup seeding through `Data/LeadgenDbSeeder.cs`
- generated the initial migration in `Migrations/`
- replaced `LeadgenMockRepository` with `LeadgenEfRepository`
- added semantic attribute routes for the main app flows
- added `lab-3/semantic-model.md`
- added `lab-3/sitemap.md`
- added repo-local skills for EF work, list pages, and edit forms
- added the `OutreachQueue` list page
- added create/edit/delete support for `ClarificationQuestion`, with create/edit using a shared partial form

What remains in the repo from Lab 3:

- EF Core + SQLite runtime persistence
- startup migration and seeding
- semantic routes such as `/`, `/mission-control`, `/missions/{id}`, `/companies/{id}`, `/contacts/{id}`, `/dossiers/{id}`, `/questions/...`, and `/outreach/queue`
- `lab-3/implementation-plan.md`
- `lab-3/semantic-model.md`
- `lab-3/sitemap.md`
- `.agents/skills/leadgen-ef/`
- `.agents/skills/leadgen-list-page/`
- `.agents/skills/leadgen-edit-form/`

### Lab 4

Lab 4 turns the EF-backed MVC app into a full CRUD application with AJAX-assisted list and form interactions.

What was done:

- added create/edit/delete support for the main EF entities
- added AJAX search to every entity list page and the outreach queue
- added `Controllers/EntitySearchController.cs` for reusable search results
- added `Controllers/LookupsController.cs` for AJAX autocomplete dropdowns
- added shared partials for search, autocomplete, delete confirmation, and custom date-time input
- added blur-triggered client validation plus server-side business validation in POST actions
- added a custom JavaScript date-time picker that displays Croatian or English date formats based on browser language
- documented the Lab 4 work in `lab-4/implementation-plan.md`, `lab-4/crud-matrix.md`, and `lab-4/completion-report.md`

### Lab 5

Lab 5 secures the MVC app and adds a DTO-based API surface over the Leadgen domain.

What was done:

- added ASP.NET Core Identity with local registration/login, `AppUser` fields for `OIB` and `JMBG`, and seeded `Admin`/`Manager` roles
- added Google external-login wiring through configuration keys without committing real secrets
- applied authorization rules across MVC pages and API endpoints
- added DTO CRUD API controllers for missions, questions, runs, assignments, agents, companies, contacts, channels, evidence, dossiers, and mission attachments
- added mission-scoped attachment metadata plus async upload/list/delete UI on mission edit pages
- added integration tests for API CRUD, missing IDs, validation failures, anonymous list access, protected operations, and admin-only deletes
- documented the Lab 5 research and implementation plan in `lab-5/implementation-plan.md`

## Current Repository State

The current repository is a single runnable MVC web application backed by EF Core, SQLite, ASP.NET Core Identity, and DTO-based API controllers.

What is active today:

- one solution with the web project and integration tests: `leadgen.sln` -> `leadgen.csproj`, `leadgen.Tests/leadgen.Tests.csproj`
- ASP.NET Core MVC controllers, views, and strongly typed view models
- ASP.NET Core Identity users, roles, local login, local registration, and Google external-login configuration
- DTO-based API controllers under `/api/...`
- domain entities in `Domain/Entities`
- enums in `Domain/Enums`
- EF Core persistence in `Data/LeadgenDbContext.cs`
- startup seeding in `Data/LeadgenDbSeeder.cs`
- startup role/user seeding in `Data/LeadgenIdentitySeeder.cs`
- schema history in `Migrations/`
- EF-backed read access through `Services/LeadgenEfRepository.cs`
- dashboard and query services in `Services/`
- dossier-style UI in `Views/` and `wwwroot/css/site.css`
- create/edit/delete workflows for the main EF entities
- AJAX search and autocomplete lookup endpoints
- mission-scoped attachment upload, listing, and deletion
- one extra projection/list workflow for the outreach queue

Current architectural summary:

- Lab 1 ideas define the domain
- Lab 2 defines the MVC shell and UX direction
- Lab 3 defines the persistence model, routing model, and first real write flow
- Lab 4 defines full CRUD, AJAX search, autocomplete, validation, and custom date-time input
- Lab 5 defines authentication, authorization, DTO APIs, mission attachments, and integration tests

## Repository Structure

```text
leadgen/
├── Controllers/         MVC controllers
├── Data/                EF Core context, seeder, and seed helpers
├── Domain/              Leadgen entities and enums
├── Migrations/          EF Core schema history
├── Models/Api           API DTO and request models
├── Services/            repository, dashboard logic, queries, simulation
├── ViewModels/          page-specific view models
├── Views/               Razor views
├── wwwroot/             CSS, JS, and static assets
├── leadgen.Tests/       API integration tests
├── lab-1/               Lab 1 planning and AI usage notes
├── lab-2/               Lab 2 planning, UX notes, and hook captures
├── lab-3/               Lab 3 plan and semantic/routing documentation
├── lab-4/               Lab 4 assignment, implementation plan, and completion notes
├── lab-5/               Lab 5 research and implementation plan
├── Program.cs           ASP.NET Core startup
├── leadgen.csproj       runnable web project
└── leadgen.sln          solution containing the app and tests
```

## How To Run

Prerequisite:

- .NET SDK 10.0

From the repository root:

```bash
dotnet restore
dotnet build leadgen.sln
dotnet run --project leadgen.csproj
```

The SQLite database file `leadgen-lab3.db` is created automatically and seeded on first run.

Seeded development users:

- `admin@leadgen.local` / `LeadgenAdmin1!` with `Admin` and `Manager`
- `manager@leadgen.local` / `LeadgenManager1!` with `Manager`

Google login is enabled when these configuration keys are populated through user secrets or environment variables:

- `Authentication:Google:ClientId`
- `Authentication:Google:ClientSecret`

Development URLs from `Properties/launchSettings.json`:

- `https://localhost:7135`
- `http://localhost:5267`

If you want automatic reload while editing:

```bash
dotnet watch run --project leadgen.csproj
```

Run the integration tests:

```bash
dotnet test leadgen.sln
```

## Main Application Coverage

The app currently includes browsing support for:

- missions
- clarification questions
- mission runs
- mission agent assignments
- swarm agents
- target companies
- target contacts
- contact channels
- evidence points
- lead dossiers

Additional Lab 3 and Lab 4 functionality includes:

- the `Outreach Queue` page at `/outreach/queue`
- create, edit, and delete flows for the main EF entities
- AJAX search endpoints under `/search/{entity}`
- autocomplete lookup endpoints under `/lookups/...`
- custom date-time inputs rendered through `Views/Shared/_DateTimeControl.cshtml`
- API CRUD endpoints under `/api/missions`, `/api/clarification-questions`, `/api/mission-runs`, `/api/mission-agent-assignments`, `/api/swarm-agents`, `/api/target-companies`, `/api/target-contacts`, `/api/contact-channels`, `/api/evidence-points`, `/api/lead-dossiers`, and `/api/mission-attachments`
- mission attachment upload/list/delete controls on mission edit pages

## Supporting Documentation

Useful repo-local references:

- `lab-1/implementation-plan.md`
- `lab-1/ai-agent-log.md`
- `lab-2/implementation-plan.md`
- `lab-2/ux-sub-agent-log.md`
- `lab-2/stitch-workflow.md`
- `lab-3/implementation-plan.md`
- `lab-3/semantic-model.md`
- `lab-3/sitemap.md`
- `lab-4/implementation-plan.md`
- `lab-4/crud-matrix.md`
- `lab-4/completion-report.md`
- `lab-5/implementation-plan.md`

## Next Likely Steps

- tighten the `lab-3/sitemap.md` page map so shared partial usage is documented more explicitly
- replace startup demo seeding with production persistence workflows when the app moves beyond lab scope
- connect real mission orchestration and live swarm progress when those integrations are ready
