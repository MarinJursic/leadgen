# Leadgen

Leadgen is an ASP.NET Core MVC application for mission-driven B2B lead research.

Instead of browsing a static lead list, the product starts from a company's Business DNA, turns that input into a structured research mission, runs a specialized swarm over companies and contacts, and outputs evidence-backed lead dossiers.

## Product shape

Leadgen is organized around three phases:

1. Intelligence Gate
   Structure product input into a mission, score confidence, and capture clarification questions.
2. Investigative Swarm
   Execute research through roles such as `Strategist`, `Scout`, `Anchor`, `Soul`, `Sentinel`, `Stitcher`, and `Sniper`.
3. Dossier Output
   Produce qualified companies, decision-makers, evidence, contact channels, and final lead dossiers.

## Lab 2 architecture

Lab 2 now lives in a single MVC project.

What is inside the web app:

- domain entities and enums
- static mock dataset from Lab 1
- LINQ query catalog used by the dashboard
- entity index and details pages for the full Leadgen object graph
- custom home page with mission, dossier, and evidence summaries
- complete navigation across all Lab 2 entity pages

## Repository structure

```text
leadgen/
├── Controllers/         MVC controllers for every Lab 2 entity
├── Data/Seed/           Lab 1 mock dataset and seed factory
├── Domain/              Leadgen entities and enums
├── Services/            repository, dashboard logic, queries, simulation
├── ViewModels/          page-specific view models
├── Views/               Razor pages
├── wwwroot/             CSS and static assets
├── lab-1/               Lab 1 notes and logs
├── lab-2/               Lab 2 plan and UX prompt material
├── Program.cs           ASP.NET Core startup
├── leadgen.csproj       only runnable project
└── leadgen.sln          solution containing only `leadgen`
```

## Running the app

```bash
dotnet run --project leadgen.csproj
```

Then open the local ASP.NET Core URL printed in the terminal.

## Lab 2 coverage

The MVC app includes Index and Details pages for:

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

## UX direction

The UI is intentionally not a default Bootstrap scaffold.

The visual direction is:

- white-first
- monochrome and editorial
- dossier-led
- mission-first rather than CRUD-first

Supporting Lab 2 UX files live in:

- `.agents/skills/leadgen-ux/`
- `lab-2/implementation-plan.md`
- `lab-2/ux-agent-prompt.md`
- `lab-2/ux-sub-agent-log.md`
- `lab-2/stitch-workflow.md`

## Next steps after Lab 2

- connect Stitch live once the local MCP credentials are ready
- replace the mock repository with persistence
- add real mission orchestration and real-time swarm updates
