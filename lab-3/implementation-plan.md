# Leadgen Lab 3 Implementation Plan

## Purpose

This document translates `Lab3.md` into a concrete implementation plan for this repository.

It is based on:

- `Lab3.md`
- `README.md`
- `leadgen.md`
- `lab-2/implementation-plan.md`
- the current MVC codebase structure
- the current domain model, seed data, services, controllers, views, and routing setup

## What This Project Is

Leadgen is not a generic CRUD sample. It is an ASP.NET Core MVC app that models an agentic B2B lead research system:

1. a user provides Business DNA
2. the system turns that into a research mission
3. a swarm of agents researches companies and people
4. the app outputs evidence-backed lead dossiers

The current MVC app already reflects that story through:

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

## Current Repository State

The current app is a Lab 2 MVC implementation with these characteristics:

- one runnable web project: `leadgen.csproj`
- domain model lives in `Domain/Entities` and `Domain/Enums`
- data comes from `Data/Seed/LeadgenSeedFactory.cs`
- runtime storage is a nested in-memory object graph exposed by `LeadgenMockRepository`
- controllers are read-only and expose only `Index` and `Details`
- routing is still the default conventional route in `Program.cs`
- the app builds successfully in its current state

Important architectural fact:

The runtime graph is currently hierarchical, not relational:

- dataset -> missions
- mission -> clarification questions, runs
- run -> assignments, companies, dossiers
- company -> contacts
- contact -> channels, evidence

Lab 3 requires converting that shape into a proper EF Core relational model.

## Lab 3 Requirements Translated To This Project

`Lab3.md` effectively asks for six deliverables:

1. make the existing model EF-ready
2. replace the mock repository with a real EF-backed repository
3. add and explain custom routing on at least 4 controller actions
4. create `semantic-model.md`
5. create `sitemap.md`
6. create and use at least one relevant skill, with the safest full-score path being EF, list-page, edit-form, and the existing UX skill

## Recommended Technical Direction

### Database choice

Recommended default for this repo: SQLite.

Reasoning:

- simplest local setup on this machine
- no Docker dependency
- no external SQL Server dependency
- easiest migration workflow for a school lab
- fully sufficient for EF, routing, forms, and documentation goals

If the professor explicitly prefers MSSQL, keep the same entity and repository plan and only swap:

- provider package
- connection string
- `UseSqlite(...)` to `UseSqlServer(...)`

### EF strategy

Use a hybrid mapping approach:

- data annotations for simple entity rules
- Fluent API for relationships, delete behavior, indexes, and special conversions

This matches the rubric while keeping the entity files readable.

### Seeding strategy

Do not force the current demo graph into `HasData(...)` unless required.

Best approach for this repository:

- keep migrations for schema management
- add a startup seeder that populates the database only when empty
- reuse `LeadgenSeedFactory` as the source of sample data

Reason:

- current seed graph is deep and uses `Guid.NewGuid()`
- `HasData(...)` is possible but becomes brittle with many related records
- runtime seeding is simpler and still demonstrates EF correctly

## Relational Model Plan

The current entities need explicit foreign keys and navigation properties.

### Entity-by-entity mapping

| Entity | Required EF changes |
| --- | --- |
| `BusinessDnaMission` | Keep `Id`. Convert `ClarificationQuestions` and `Runs` to `virtual ICollection<T>`. Handle `SurfaceTags` via JSON/value-conversion or a separate table. |
| `ClarificationQuestion` | Add `BusinessDnaMissionId` and `virtual BusinessDnaMission Mission`. |
| `MissionRun` | Keep `BusinessDnaMissionId`, add `virtual BusinessDnaMission Mission`, convert child lists to `virtual ICollection<T>`. |
| `MissionAgentAssignment` | Keep `MissionRunId` and `SwarmAgentId`, add `virtual MissionRun MissionRun` and `virtual SwarmAgent SwarmAgent`. |
| `SwarmAgent` | Convert `MissionAssignments` to `virtual ICollection<MissionAgentAssignment>`. |
| `TargetCompany` | Add `MissionRunId` and `virtual MissionRun MissionRun`, convert `Contacts` to `virtual ICollection<TargetContact>`. |
| `TargetContact` | Add `TargetCompanyId` and `virtual TargetCompany TargetCompany`, convert `ContactChannels` and `EvidencePoints` to `virtual ICollection<T>`. |
| `ContactChannel` | Add `TargetContactId` and `virtual TargetContact TargetContact`. |
| `EvidencePoint` | Add `TargetContactId` and `virtual TargetContact TargetContact`. |
| `LeadDossier` | Keep `MissionRunId`, `TargetCompanyId`, `TargetContactId`, add `virtual MissionRun MissionRun`, `virtual TargetCompany TargetCompany`, `virtual TargetContact TargetContact`. |

### Special-case modeling decisions

`BusinessDnaMission.SurfaceTags` is the only property that is not naturally tabular.

Best option for Lab 3:

- keep `List<string> SurfaceTags`
- map it with a value converter to a JSON text column

This keeps the UI and seed factory stable without introducing an unnecessary extra entity.

### Annotation and configuration checklist

Add where appropriate:

- `[Key]`
- `[Required]`
- `[MaxLength(...)]`
- `[Precision(...)]` on decimals
- `[ForeignKey(...)]` where it genuinely improves clarity

Configure in Fluent API:

- one-to-many relationships
- required vs optional relationships
- delete behavior
- `SurfaceTags` conversion
- indexes such as `RunCode` and `SwarmAgent.CodeName`

## DbContext and Infrastructure Plan

### New files

- `Data/LeadgenDbContext.cs`
- `Data/LeadgenDbSeeder.cs`
- `Data/Configurations/*.cs` for non-trivial entity mappings
- `Migrations/*`

### `LeadgenDbContext` responsibilities

- expose one `DbSet<T>` per entity
- apply entity configurations
- configure special conversions

Required `DbSet<T>` entries:

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

### Package plan

If using SQLite:

- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

If using SQL Server:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

### Configuration plan

Add a connection string to `appsettings.json`.

Recommended SQLite example:

```json
"ConnectionStrings": {
  "LeadgenDb": "Data Source=leadgen-lab3.db"
}
```

Update `Program.cs` to:

- register `LeadgenDbContext`
- replace the singleton mock repository with a scoped EF repository
- keep MVC registration
- run migrations and seed data at startup

## Repository and Service Refactor Plan

### Problem in the current code

`LeadgenDashboardService` and the controllers currently depend on a mock repository that reconstructs everything from one in-memory dataset.

That is acceptable for Lab 2, but it is the wrong shape for EF.

### Recommended repository target

Replace `LeadgenMockRepository` with `LeadgenEfRepository`.

Recommended contract changes:

- move to async methods
- stop exposing `GetDataset()` as the main runtime dependency
- let repository methods return only the entities needed by each controller/service

### Concrete repository work

1. Keep `ILeadgenReadRepository` as the seam so controllers do not change all at once.
2. Replace sync methods with async versions where practical.
3. Implement EF queries with focused `Include(...)` usage.
4. Avoid loading the entire graph for every request.

### Dashboard service refactor

`LeadgenDashboardService` should stop depending on a whole-dataset object.

Best implementation:

- query counts directly from EF
- project featured missions, top leads, and recent signals with targeted queries
- keep `LeadgenQueryCatalog` only if it remains useful after being rewritten for EF-friendly projections

## Routing Plan

### Current routing baseline

Current routing is only:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

That route should remain as the fallback conventional route.

### Recommended custom routing approach

Use attribute routing for the canonical, semantic pages.

Reasoning:

- easier to explain in oral examination
- easier to map in `sitemap.md`
- keeps route intent close to the action
- satisfies the requirement for non-default routing on at least 4 actions

### Recommended canonical routes

| Controller action | Recommended URL | View |
| --- | --- | --- |
| `Home.Index` | `/` | `Views/Home/Index.cshtml` |
| `Home.Mission` | `/mission-control` | `Views/Home/Mission.cshtml` |
| `Missions.Index` | `/missions` | `Views/Missions/Index.cshtml` |
| `Missions.Details(Guid id)` | `/missions/{id:guid}` | `Views/Missions/Details.cshtml` |
| `TargetCompanies.Index` | `/companies` | `Views/TargetCompanies/Index.cshtml` |
| `TargetCompanies.Details(Guid id)` | `/companies/{id:guid}` | `Views/TargetCompanies/Details.cshtml` |
| `TargetContacts.Index` | `/contacts` | `Views/TargetContacts/Index.cshtml` |
| `TargetContacts.Details(Guid id)` | `/contacts/{id:guid}` | `Views/TargetContacts/Details.cshtml` |
| `LeadDossiers.Index` | `/dossiers` | `Views/LeadDossiers/Index.cshtml` |
| `LeadDossiers.Details(Guid id)` | `/dossiers/{id:guid}` | `Views/LeadDossiers/Details.cshtml` |

This gives more than enough custom-routed actions while staying consistent with the product language.

### Route constraints to demonstrate

Use route constraints in the final implementation:

- `{id:guid}` for detail pages
- optional query string `dna` on `/mission-control?dna=...`

## Documentation Deliverables Plan

### `lab-3/semantic-model.md`

This file should be generated after the EF model is finalized.

It should contain:

- every entity/table
- primary key
- main scalar properties
- foreign keys
- cardinality of each relationship
- note about the `SurfaceTags` storage strategy

### `lab-3/sitemap.md`

This file should be generated after routing is finalized.

It should contain, for every accessible page:

- URL
- controller
- action
- route type: conventional or attribute
- input parameters
- Razor view used

It must include:

- home page
- mission canvas
- every entity `Index`
- every entity `Details`
- privacy page
- error page
- any new list/edit/create routes added for Lab 3

## Skills Plan

The existing repo already contains:

- `.agents/skills/leadgen-ux/SKILL.md`

For Lab 3, the safest full-coverage plan is to add three more repo-local skills:

- `.agents/skills/leadgen-ef/SKILL.md`
- `.agents/skills/leadgen-list-page/SKILL.md`
- `.agents/skills/leadgen-edit-form/SKILL.md`

### `leadgen-ef` skill scope

Use when:

- changing entity mappings
- adding foreign keys
- editing `DbContext`
- creating migrations
- updating seed logic

### `leadgen-list-page` skill scope

Use when:

- creating a new read-only list page
- wiring controller, query, view model, view, and navigation

Recommended demo page:

- `OutreachQueueController.Index`
- canonical route: `/outreach/queue`

Why this page:

- it extends the app without inventing a new domain
- it reuses real dossier data
- it naturally demonstrates joins and projections in EF

### `leadgen-edit-form` skill scope

Use when:

- creating create/edit pages with model binding, validation, and select lists

Recommended first edit form:

- `ClarificationQuestions/Create`
- `ClarificationQuestions/Edit/{id:guid}`

Why this is the best first form:

- relation to `BusinessDnaMission` proves EF foreign keys
- fields are moderate in size
- it fits the real product flow
- it can reuse one partial form view

### Partial view recommendation

Create:

- `Views/ClarificationQuestions/_Form.cshtml`

Use it from both:

- `Create.cshtml`
- `Edit.cshtml`

This aligns with the partial-view concepts discussed in `Lab3.md`.

## Step-by-Step Execution Plan

### Phase 1. Prepare the persistence layer

1. Add EF provider and design packages.
2. Add a `ConnectionStrings` section to `appsettings.json`.
3. Create `LeadgenDbContext`.
4. Add one `DbSet<T>` for each entity.
5. Register the context in `Program.cs`.

### Phase 2. Make entities EF-ready

1. Add annotations to every entity.
2. Add missing foreign key properties.
3. Add missing reference navigation properties.
4. Convert collection properties to `virtual ICollection<T>`.
5. Configure `SurfaceTags` conversion.

### Phase 3. Replace the mock repository

1. Create `LeadgenEfRepository`.
2. Register it instead of `LeadgenMockRepository`.
3. Refactor controllers to use EF-backed methods.
4. Refactor `LeadgenDashboardService` to query EF directly or through repository projections.

### Phase 4. Seed and migrate

1. Add `LeadgenDbSeeder`.
2. Reuse `LeadgenSeedFactory` for initial sample insertion.
3. Create the initial migration.
4. Apply the migration.
5. Verify that the app boots against the database and shows the same demo data.

### Phase 5. Add custom routing

1. Keep the default conventional route.
2. Add attribute routes to the canonical actions listed above.
3. Update navigation links if route assumptions changed.
4. Verify at least 4 actions use non-default routing.

### Phase 6. Add a new list page

1. Add `OutreachQueueController`.
2. Add a query that returns ready-for-outreach dossiers with mission, company, and contact context.
3. Add `Views/OutreachQueue/Index.cshtml`.
4. Link it from navigation or the home vault.

### Phase 7. Add create/edit form support

1. Add `Create` and `Edit` actions to `ClarificationQuestionsController`.
2. Add a form view model if needed.
3. Add `_Form.cshtml`, `Create.cshtml`, and `Edit.cshtml`.
4. Use tag helpers like `asp-for`, `asp-action`, and `asp-route-id`.
5. Add validation summary and field validation.

### Phase 8. Write the documentation files

1. Write `lab-3/semantic-model.md`.
2. Write `lab-3/sitemap.md`.
3. Ensure the route map reflects the final code, not the pre-change state.

### Phase 9. Write the skill files

1. Add `leadgen-ef`.
2. Add `leadgen-list-page`.
3. Add `leadgen-edit-form`.
4. Keep `leadgen-ux` unchanged unless Lab 3 UI additions require polish.

## Commands To Use During Implementation

Example package install path for SQLite:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

If `dotnet ef` is not available:

```bash
dotnet tool install --global dotnet-ef
```

Create and apply migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Build and verify:

```bash
dotnet build leadgen.sln
dotnet run --project leadgen.csproj
```

## Acceptance Checklist

Lab 3 is complete when all of the following are true:

- EF packages are installed
- `LeadgenDbContext` is registered and used
- the app no longer depends on `LeadgenMockRepository` at runtime
- the initial migration exists and applies successfully
- database-backed data loads in the MVC app
- at least 4 controller actions use custom routing
- `lab-3/semantic-model.md` exists and matches the EF model
- `lab-3/sitemap.md` exists and matches the final routes
- at least one new list page exists
- at least one new create/edit form flow exists
- new repo-local skill files exist and are specific to this app

## Risks and Pitfalls

1. Do not keep the old nested graph assumptions inside the EF repository. That recreates the mock architecture and defeats the point of Lab 3.
2. Do not overuse `Include(...)` everywhere. Use focused loading and projections.
3. Do not leave `SurfaceTags` unresolved. It is the one property that needs an explicit storage decision.
4. Do not generate `semantic-model.md` or `sitemap.md` before the code is stable, or the documentation will immediately drift.
5. Do not choose a form target that is too relationally complex for the first pass. `ClarificationQuestion` is a safer starting point than `MissionRun` or `LeadDossier`.

## Recommended Implementation Order

If this work is done under time pressure, use this order:

1. EF packages, connection string, `DbContext`
2. entity FK/navigation refactor
3. EF repository replacement
4. migration and startup seeding
5. custom routing
6. new list page
7. create/edit form page
8. `semantic-model.md`
9. `sitemap.md`
10. skill files

This order minimizes rework because the documentation and skills can then describe the final architecture rather than an intermediate state.
