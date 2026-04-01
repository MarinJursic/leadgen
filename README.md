# Leadgen

Leadgen is an ASP.NET Core MVC product for agentic B2B lead generation. Instead of querying a static lead database with filters, the product starts from a company's "Business DNA", turns that into a structured mission, runs a coordinated research swarm, and produces evidence-backed lead dossiers with outreach context.

## Vision

The product is built around three phases:

1. Intelligence Gate  
   Turn vague product input into a structured mission with confidence scoring and clarification questions.
2. Investigative Swarm  
   Run specialized agents such as `Strategist`, `Scout`, `Anchor`, `Soul`, `Sentinel`, and `Stitcher` to find companies, decision-makers, qualification signals, and verified contact channels.
3. Dossier and Archive  
   Produce prioritized lead dossiers with evidence, scoring, and a suggested outreach angle.

## Current Status

This repository currently contains:

- an ASP.NET Core MVC web app scaffold that will become the main product host
- a shared `Leadgen.Model` class library with the core Leadgen domain model
- a `Leadgen.Lab1Runner` console app used to seed in-memory data, run product-relevant LINQ queries, and demonstrate async swarm simulation

The current implementation focuses on the first real domain slice of the product:

- `BusinessDnaMission`
- `MissionRun`
- `SwarmAgent`
- `TargetCompany`
- `TargetContact`
- `ContactChannel`
- `EvidencePoint`
- `LeadDossier`

## Repository Structure

```text
leadgen/
├── Controllers/              ASP.NET Core MVC controllers
├── Models/                   MVC view models
├── Views/                    Razor views
├── wwwroot/                  static assets
├── Leadgen.Model/            shared domain model
├── Leadgen.Lab1Runner/       console runner for seed data, LINQ, and async demo
├── Program.cs                ASP.NET Core MVC startup
├── leadgen.csproj            web project
└── leadgen.sln               solution file
```

## What Is Implemented

### Shared model

The shared model captures the product concepts that matter across the PRDs:

- mission intake and clarification
- mission execution runs
- agent roles and assignments
- candidate companies and target contacts
- evidence-backed qualification
- final dossier output

### Lab 1 runner

The runner currently demonstrates:

- three seeded Leadgen missions from different business domains
- realistic `1:N` and `N:N` relationships
- ten meaningful LINQ queries over the Leadgen object graph
- an `async/await` simulation of a staged swarm flow:
  `Scout` + `Sentinel` -> `Anchor` -> `Soul`

## Running the Project

### Run the Lab 1 runner

```bash
dotnet run --project Leadgen.Lab1Runner/Leadgen.Lab1Runner.csproj
```

### Run the MVC app

```bash
dotnet run --project leadgen.csproj
```

Then open the local ASP.NET Core URL shown in the terminal.

## Technical Direction

The repository is intentionally aligned with the Microsoft stack that best fits the product direction:

- ASP.NET Core MVC for the web host and separation of concerns
- LINQ for querying the in-memory and future persisted mission graph
- `async` / `await` for staged and concurrent agent work
- SignalR for future real-time swarm telemetry
- Microsoft Agent Framework for future agent orchestration

## Near-Term Roadmap

1. Wire the shared Leadgen model into the MVC app instead of the default scaffolded home page.
2. Add service-layer mission orchestration inside the web project.
3. Introduce persistence for missions, runs, evidence, and dossiers.
4. Add real-time mission progress updates with SignalR.
5. Replace the current async simulator with real agent/provider integrations.

## Documentation Strategy

This repository tracks only `README.md` as committed Markdown documentation. Other working notes and lab-specific markdown files are kept local and ignored by design.

## Research Basis

The current direction is informed by official Microsoft documentation for:

- ASP.NET Core MVC  
  https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-9.0
- LINQ in C#  
  https://learn.microsoft.com/en-us/dotnet/csharp/linq/
- Asynchronous programming with `async` and `await`  
  https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
- ASP.NET Core SignalR  
  https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0
- Microsoft Agent Framework  
  https://learn.microsoft.com/en-gb/agent-framework/overview/
