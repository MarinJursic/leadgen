# Leadgen Lab 2 Implementation Plan

## Purpose

This document defines how Leadgen should satisfy `lab2.md` while staying faithful to the actual product idea.

Research basis used for this plan:

- `lab2.md`
- `lab1.md`
- `leadgen.md`
- `README.md`
- `Leadgen Intelligence Swarm Project Plan.md`
- `NET MVC Agentic Swarm Project Research.md`
- the current repository state

## What Leadgen is

Leadgen is an MVC product for agentic B2B lead generation.

The system does not start from a spreadsheet of companies. It starts from structured Business DNA, converts that into a mission, runs a multi-agent investigation, and outputs evidence-backed lead dossiers.

The product model is built around:

1. mission intake and clarification
2. swarm execution runs and assignments
3. companies, contacts, channels, and evidence
4. final dossiers

That framing matters for Lab 2 because the UI should feel like an intelligence system, not a generic CRUD sample.

## Lab 2 requirements translated into Leadgen

From `lab2.md`, the practical delivery target is:

- one ASP.NET Core MVC application
- static mock repository data from Lab 1
- Index and Details pages for every entity
- one custom page, implemented here as the home dashboard
- full navigation:
  - primary menu
  - list-to-details links
  - breadcrumbs
- unique, non-default UX
- committed UX sub-agent prompt and usage log
- a repo-local UX agent definition and an invocation-proof file

## Architecture decision

For Lab 2, the right shape is a single MVC project.

Reasoning:

1. the grading focus is MVC, binding, HTML, and navigation
2. the old console runner adds no Lab 2 grading value
3. the model, seed data, and query logic are now part of the web app itself
4. one project is easier to explain during oral examination

## Final target structure

```text
leadgen/
├── Controllers/
├── Data/Seed/
├── Domain/
│   ├── Entities/
│   └── Enums/
├── Services/
│   ├── Queries/
│   └── Simulation/
├── ViewModels/
├── Views/
├── wwwroot/
├── lab-1/
├── lab-2/
├── Program.cs
├── leadgen.csproj
└── leadgen.sln
```

## Concrete implementation plan

### 1. Consolidate Lab 1 into MVC

- move domain entities into the MVC project
- move enums into the MVC project
- move Lab 1 seed data into `Data/Seed`
- move LINQ query helpers into `Services/Queries`
- keep optional async simulation under `Services/Simulation`
- remove the extra project dependency graph so `leadgen.csproj` is the only app project

### 2. Keep mock repository as the Lab 2 data source

- register one read-only repository in DI
- seed three realistic Leadgen missions
- preserve the full object graph from Lab 1:
  - missions
  - clarification questions
  - runs
  - assignments
  - agents
  - companies
  - contacts
  - channels
  - evidence
  - dossiers

### 3. Implement complete entity coverage

Every entity should have:

- `Index` action
- `Details` action
- Razor `Index.cshtml`
- Razor `Details.cshtml`

Entity scope:

- Missions
- ClarificationQuestions
- MissionRuns
- MissionAgentAssignments
- SwarmAgents
- TargetCompanies
- TargetContacts
- ContactChannels
- EvidencePoints
- LeadDossiers

### 4. Implement navigation that satisfies the rubric

- primary layout navigation should expose every Lab 2 entity area
- list pages must link to detail pages
- detail pages must include breadcrumbs
- related-entity links should be preserved where useful
- home page should act as a map of the whole Leadgen graph

### 5. Use a custom home page instead of a template landing page

The home page should explain the product and summarize the data:

- mission count
- agent count
- company count
- contact count
- dossier count
- featured missions
- top dossiers
- recent evidence signals
- average score by product surface
- direct links to every entity section

### 6. Maintain a unique UX direction

The visual language should remain:

- white-first
- monochrome
- editorial
- premium
- dossier-led

Avoid:

- default Bootstrap landing layouts
- colorful SaaS-dashboard clichés
- CRUD-only visual framing

## Verification checklist

Lab 2 is considered complete when all of the following are true:

- `leadgen.sln` contains only the MVC project
- `leadgen.csproj` builds without project references to the old Lab 1 structure
- the app runs from a single `dotnet run --project leadgen.csproj` entry point
- every entity page has Index and Details coverage
- the top navigation exposes every entity section
- the home page acts as the custom page for the lab
- `lab-2/ux-agent-prompt.md` remains committed
- `.agents/skills/leadgen-ux/` contains the UX sub-agent definition
- `lab-2/ux-sub-agent-log.md` records a real invocation and applied follow-up changes

## Deferred after Lab 2

These are valid next steps, but not blockers for the lab:

- live Stitch MCP execution once credentials are configured
- persistence instead of the mock repository
- real mission orchestration and real-time updates
