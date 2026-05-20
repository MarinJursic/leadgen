# Leadgen Lab 1 Implementation Plan

## 1. Purpose of this document

This plan expands Lab 1 around the **actual Leadgen product requirements**, not around a generic C# exercise.

The goal is to make sure that everything implemented for Lab 1:

- satisfies the rubric from `lab1.md`
- matches the architecture described in the Leadgen PRDs
- becomes reusable in the future ASP.NET MVC product

This means the lab should establish the **core Leadgen domain model**, a **meaningful set of LINQ queries**, and a small **async simulation** that mirrors the future swarm behavior.

## 2. Product understanding from the Leadgen documents

After reviewing:

- `leadgen.md`
- `Leadgen Intelligence Swarm Project Plan.md`
- `NET MVC Agentic Swarm Project Research.md`
- `lab1.md`

the product can be summarized as follows.

### 2.1 What Leadgen is

Leadgen is an **ASP.NET MVC intelligence platform** for B2B lead generation. Instead of searching static lead databases with filters, the user describes their product as **Business DNA** and the system turns that into an investigation mission.

The product then:

1. structures the user input into a mission
2. dispatches specialized research agents
3. finds companies and relevant people
4. validates signals and contact channels
5. outputs a scored lead dossier with supporting evidence

### 2.2 Product phases that matter for Lab 1

#### Phase 1. Intelligence Gate

This phase converts raw user input into a structured mission using:

- `Mechanic`
- `Surface`
- `Persona`
- `Villain`
- `Delta`
- confidence scoring
- clarification questions

Lab 1 must therefore model:

- a structured mission object
- confidence level
- clarification questions
- status transitions such as draft, needs clarification, ready

#### Phase 2. Investigative Swarm

This phase uses a master-worker topology:

- `Strategist`
- `Scout`
- `Anchor`
- `Soul`
- `Sentinel`
- `Stitcher`
- optionally `Sniper`

Lab 1 must therefore model:

- agents and their roles
- mission execution runs
- assignments between runs and agents
- companies, contacts, signals, and evidence

#### Phase 3. Dossier, observability, archive

This phase produces:

- scored dossiers
- verified contact vectors
- evidence vault
- future real-time monitoring and historical archive

Lab 1 must therefore model:

- dossier output
- contact channels
- evidence with timestamps and source info
- enough status and audit data to evolve later into dashboard/archive features

### 2.3 Non-negotiable product characteristics

The product docs are consistent on several points. Lab 1 should preserve them.

#### A. Leadgen is evidence-based, not just list-based

The product is not supposed to return anonymous spreadsheet rows. It should return **why** someone is a good lead and **what evidence proves it**.

Implication for Lab 1:

- create `EvidencePoint`
- create `LeadDossier`
- link contacts to evidence

#### B. Leadgen is mission-driven, not CRUD-driven

The user gives a mission, the system investigates, then produces results. The product is not centered around manual data entry forms for companies and contacts.

Implication for Lab 1:

- make `BusinessDnaMission` the root
- make `MissionRun` the execution unit
- do not start the model from `Customer`, `Company`, or `User` only

#### C. Leadgen is multi-agent and asynchronous

The PRD repeatedly describes parallel agent work and staged discovery.

Implication for Lab 1:

- model agent assignments
- use async simulation that mirrors concurrent workers
- use `Task.WhenAll(...)` to reflect future swarm behavior

#### D. Leadgen depends on traceability and auditability

The docs emphasize evidence vaults, source URLs, timestamps, and trust in outputs.

Implication for Lab 1:

- include source platform, source URL, timestamps, confidence values
- include `DateTime` fields intentionally, not just to satisfy the rubric

## 3. Lab 1 constraints from `lab1.md`

The lab requires:

- a public GitHub repository
- all assessed code on `main`
- root folder `lab-1`
- AI usage log in `lab-1`
- at least 7 classes
- at least 4 complex classes with more than 5 properties
- at least one enum
- at least one `DateTime`
- proper `1:N` and `N:N` relationships
- at least 3 branched main/root objects
- meaningful LINQ queries
- ability to explain and modify LINQ
- understanding of `async/await`

The strongest way to satisfy those constraints while staying product-relevant is to treat Lab 1 as a **domain slice of Leadgen Phase 1 + Phase 2 + dossier output**.

## 4. Research-backed technical basis

The recommendations below are aligned with official Microsoft documentation and with the current repo state.

### 4.1 Why keep the MVC app and add a model project

The current repo is still essentially the default ASP.NET Core MVC scaffold. That means the main missing piece is the domain model, not the UI.

Research basis:

- ASP.NET Core MVC organizes an app around controllers, views, and models, which supports separation of concerns.
- .NET supports project-to-project references cleanly, so a model library can be shared by multiple projects.

Why that matters for Leadgen:

- the future MVC app will need the same domain model in controllers, services, background workers, and real-time features
- a reusable model project avoids rewriting the same classes later

### 4.2 Why a console runner is the right Lab 1 delivery vehicle

The lab explicitly asks for a “main program” that fills objects with data and runs LINQ over them.

Research basis:

- a console app is the simplest and clearest executable host for C# program flow
- project references let the console app reuse the same model library as the web app

Why that matters for Leadgen:

- the console runner becomes a safe sandbox for seeding Leadgen missions and demonstrating analytics logic
- it proves the model before the MVC UI is built

### 4.3 Why LINQ should be written over domain collections instead of toy lists

Research basis:

- LINQ is designed for querying sequences in a composable way
- it is directly useful for filtering, grouping, projection, and aggregation over object graphs

Why that matters for Leadgen:

- future product features like lead ranking, readiness checks, evidence summaries, and shortlist views are all query problems
- if Lab 1 LINQ is written over the real domain, it can later move into services or repositories with minimal redesign

### 4.4 Why async simulation belongs in Lab 1

Research basis:

- `async` and `await` are the standard .NET tools for non-blocking asynchronous work
- `Task.WhenAll(...)` is the natural pattern when multiple independent operations can run concurrently

Why that matters for Leadgen:

- the future product explicitly runs multiple agents in parallel
- Scout and Sentinel are independent early-stage workers
- later Anchor, Soul, and Stitcher steps depend on prior outputs

So the async demo should simulate the real product’s execution pattern, not a random delay example.

### 4.5 Why later phases should evolve toward hosted/background services

Research basis:

- ASP.NET Core supports background tasks through hosted services

Why that matters for Leadgen:

- long-running research missions should not block MVC request threads
- Lab 1’s async simulation is the first conceptual step toward later background orchestration

### 4.6 Why later phases should evolve toward SignalR and workflow orchestration

Research basis:

- ASP.NET Core SignalR is the standard real-time option in the Microsoft stack
- Microsoft Agent Framework supports agents and graph-based workflows for multi-step, multi-agent systems

Why that matters for Leadgen:

- the product docs describe a live swarm map and multi-agent execution graph
- Lab 1 does not implement those yet, but it should preserve the domain concepts they need

### 4.7 Enum design adjustments after review

Research basis:

- Microsoft’s C# enum guidance says enums should be used when a value must be one of a **fixed set of options**.
- Microsoft’s flags guidance says `[Flags]` is appropriate only when the enum represents a **combination of known choices**.

Implication for Leadgen:

- `MissionStatus`, `AgentRole`, and `ContactChannelType` are good enum candidates because they are small, stable vocabularies.
- the earlier `EvidenceClassification` proposal was too domain-specific for a broad leadgen platform, so it should become a **generic evidence enum plus a flexible label/tag field**
- `FundingStage` should not live in the core model as a fixed enum because it overfits startup/SaaS use cases
- `SurfaceType` should not be a simple single-value enum because product surfaces are often multi-valued and open-ended

Design decision:

- keep enums only for truly closed vocabularies
- use strings, tags, or small value-object style structures for open-ended business descriptors

## 5. PRD-to-Lab traceability matrix

This is the key section for keeping the lab relevant to the product.

| PRD concept | Meaning in Leadgen | Lab 1 artifact | Why this belongs now |
| :---- | :---- | :---- | :---- |
| Business DNA | Structured product description from user | `BusinessDnaMission` | This is the root of the entire product |
| Confidence threshold | Determines if research can start | `ConfidenceScore`, `MissionStatus` | Needed for Phase 1 logic |
| Clarification loop | Ask targeted questions before research | `ClarificationQuestion` | Needed to model “not ready yet” missions |
| Strategist + worker swarm | Multi-agent execution | `SwarmAgent`, `MissionAgentAssignment`, `MissionRun` | Needed to represent the execution topology |
| Broad company search | Identify candidate organizations | `TargetCompany` | Needed for the scouting output |
| Persona identification | Find the right decision-maker | `TargetContact` | Needed to model Anchor output |
| Pain signals / sentiment | Find reasons for outreach | `EvidencePoint` | Needed to model Soul output |
| Contact verification | Capture outreach channels | `ContactChannel` | Needed to model Stitcher/Apollo-style enrichment |
| Final score 10 dossier | Actionable output for user | `LeadDossier` | This is the product deliverable |
| Evidence vault | Proof and auditability | source URL, timestamp, summary, confidence fields | Needed for trust and future archive features |
| Real-time swarm flow | Future live dashboard | `MissionStatus`, `AssignedAtUtc`, agent status-related fields | Domain support now, UI later |

## 6. Recommended solution structure

### 6.1 Proposed projects

- keep the current ASP.NET MVC web app in the repository root: `leadgen`
- add `Leadgen.Model` class library
- add `Leadgen.Lab1Runner` console application
- keep all lab documents and logs in root folder `lab-1`

### 6.2 Why each project exists

#### `leadgen` web project

Why it exists:

- it is the future product host
- controllers, views, SignalR hubs, background wiring, and dependency injection will live here

Why it is not the main Lab 1 focus:

- the current product gap is not “missing UI”
- the current gap is “missing core domain model”

#### `Leadgen.Model`

Why it exists:

- stores the domain classes and enums
- keeps product semantics in one place
- can be referenced from MVC, console runner, tests, and later services

Why this directly supports the PRD:

- the PRD describes stable domain concepts such as missions, agents, companies, contacts, evidence, and dossiers
- those concepts should not be tied to one UI layer

#### `Leadgen.Lab1Runner`

Why it exists:

- provides the “main program” requested by the lab
- seeds in-memory Leadgen data
- runs product-relevant LINQ queries
- demonstrates async/await behavior

Why this directly supports the PRD:

- it lets you simulate the mission lifecycle and agent execution before web implementation exists

#### `lab-1`

Why it exists:

- required by the lab brief
- should contain `implementation-plan.md`
- should contain `ai-agent-log.md`

## 7. Detailed domain model with product rationale

This section explains not only what to implement, but why each class exists in Leadgen.

### 7.1 Enums

#### `MissionStatus`

Suggested values:

- `Draft`
- `NeedsClarification`
- `ReadyForResearch`
- `Running`
- `Completed`
- `Archived`

Why this exists in the product:

- the mission goes through a clear lifecycle from intake to execution to archive

Why this matters in Lab 1:

- status-based LINQ queries become meaningful and product-realistic

#### `AgentRole`

Suggested values:

- `Strategist`
- `Scout`
- `Anchor`
- `Soul`
- `Sentinel`
- `Stitcher`
- `Sniper`

Why this exists in the product:

- the PRD defines these exact specialized agent roles

Why this matters in Lab 1:

- prevents the lab from drifting into generic “worker/task” modeling

#### `EvidenceKind`

Suggested values:

- `Signal`
- `Profile`
- `Organization`
- `Contact`
- `Relationship`
- `Verification`
- `Content`
- `Other`

Why this exists in the product:

- evidence is not uniform, but the core product should not hardcode every possible business-specific signal type into an enum

Why this matters in Lab 1:

- keeps the enum generic enough for many Leadgen use cases
- lets the specific meaning live in free text such as `Label`, for example `Pain signal`, `Hiring signal`, `Expansion signal`, or `Tech stack mention`

#### `ContactChannelType`

Suggested values:

- `WorkEmail`
- `PersonalEmail`
- `Phone`
- `LinkedIn`
- `X`
- `GitHub`
- `Reddit`

Why this exists in the product:

- the PRD explicitly describes multiple contact vectors and identity links

Why this matters in Lab 1:

- lets you query “outreach-ready” contacts instead of just contacts with a name

#### Enum decision summary

Use enums in the core model only where the vocabulary is truly bounded:

- keep `MissionStatus`
- keep `AgentRole`
- keep `ContactChannelType`
- keep a generic `EvidenceKind`

Do not use enums in the core model for:

- `Surface`
- company funding/maturity stage

Reason:

- those concepts are too open-ended, too contextual, or too multi-valued for a rigid enum in a general leadgen platform

### 7.2 Classes

#### 1. `BusinessDnaMission` (complex)

Purpose:

- represents the structured output of the Intelligence Gate

Suggested properties:

- `Guid Id`
- `string MissionName`
- `string ProductName`
- `string Mechanic`
- `string PrimarySurface`
- `List<string> SurfaceTags`
- `string Persona`
- `string Villain`
- `string Delta`
- `decimal ConfidenceScore`
- `DateTime CreatedAtUtc`
- `MissionStatus Status`
- `List<ClarificationQuestion> ClarificationQuestions`
- `List<MissionRun> Runs`

Why this class is necessary:

- this is the product’s true root entity
- the PRD is built around transforming vague product input into structured mission data

Why these properties matter:

- `Mechanic`, `PrimarySurface`, `SurfaceTags`, `Persona`, `Villain`, and `Delta` map directly to the PRD ontology
- `ConfidenceScore` maps directly to the gatekeeping threshold logic
- `Status` lets the mission move from clarification to research

Why `PrimarySurface` + `SurfaceTags` is better than `SurfaceType`:

- the PRD’s “Surface” slot is conceptually important, but it is not a clean single-choice enum in a real product
- a product may be a web app, API, and plugin at the same time
- storing a primary surface plus tags is more flexible for broad Leadgen use cases while remaining simple for Lab 1

Future refinement note:

- if you later decide to normalize surfaces into a small, known, combinable set, a `[Flags]` enum becomes a valid option
- until then, strings/tags are the safer core-model choice because the concept is broader than a fixed single-value enum

#### 2. `ClarificationQuestion`

Purpose:

- stores the targeted follow-up questions created when the mission is under-specified

Suggested properties:

- `Guid Id`
- `string SlotName`
- `string Prompt`
- `string Reason`
- `bool IsAnswered`
- `string? Answer`
- `DateTime CreatedAtUtc`
- `DateTime? AnsweredAtUtc`

Why this class is necessary:

- the PRD explicitly describes recursive questioning for missing or weak slots

Why these properties matter:

- `SlotName` preserves which ontology slot is weak
- `Reason` explains why the question exists
- `IsAnswered` and timestamps support readiness checks

#### 3. `MissionRun` (complex)

Purpose:

- represents one concrete execution of a mission

Suggested properties:

- `Guid Id`
- `string RunCode`
- `Guid BusinessDnaMissionId`
- `DateTime StartedAtUtc`
- `DateTime? CompletedAtUtc`
- `MissionStatus Status`
- `string SearchRegion`
- `int TokenBudget`
- `decimal EstimatedCostUsd`
- `List<MissionAgentAssignment> AgentAssignments`
- `List<TargetCompany> TargetCompanies`
- `List<LeadDossier> LeadDossiers`

Why this class is necessary:

- the product does not just “have missions”; it executes them
- a mission may be run multiple times later for refresh, retry, or diffing

Why these properties matter:

- `TokenBudget` and `EstimatedCostUsd` reflect the PRD’s cost-aware swarm execution
- `StartedAtUtc` and `CompletedAtUtc` support observability and archive use cases

#### 4. `SwarmAgent` (complex)

Purpose:

- models the specialized agents in the PRD

Suggested properties:

- `Guid Id`
- `string CodeName`
- `AgentRole Role`
- `string Provider`
- `decimal Temperature`
- `int MaxConcurrentTasks`
- `bool IsActive`
- `DateTime LastHeartbeatUtc`
- `string CurrentFocus`
- `List<MissionAgentAssignment> MissionAssignments`

Why this class is necessary:

- Leadgen’s key differentiator is the agentic swarm architecture

Why these properties matter:

- `Role` maps directly to product behavior
- `MaxConcurrentTasks` and `CurrentFocus` support later scheduling/telemetry thinking
- `LastHeartbeatUtc` supports future dashboard ideas and gives a meaningful `DateTime`

#### 5. `MissionAgentAssignment`

Purpose:

- join entity between mission runs and agents

Suggested properties:

- `Guid Id`
- `Guid MissionRunId`
- `Guid SwarmAgentId`
- `DateTime AssignedAtUtc`
- `string Responsibility`
- `int TokenBudget`
- `MissionStatus Status`

Why this class is necessary:

- a run has multiple agents
- an agent can participate in many runs
- this is the cleanest and most product-faithful `N:N` relationship in the model

Why these properties matter:

- `Responsibility` explains the concrete task delegated to the agent
- `TokenBudget` reflects controlled swarm execution

#### 6. `TargetCompany` (complex)

Purpose:

- stores candidate companies discovered during scouting

Suggested properties:

- `Guid Id`
- `string Name`
- `string Domain`
- `string Industry`
- `string HeadquartersCity`
- `string HeadquartersCountry`
- `string? OrganizationStageLabel`
- `DateTime? LastSignalAtUtc`
- `int EmployeeCount`
- `bool IsHeadquartersVerified`
- `decimal MatchScore`
- `List<TargetContact> Contacts`

Why this class is necessary:

- the scouting stage produces organizations before it produces individual people

Why these properties matter:

- `Industry`, `OrganizationStageLabel`, and `EmployeeCount` are realistic qualification signals
- `IsHeadquartersVerified` matches the PRD’s verified company evidence idea
- `MatchScore` supports shortlisting queries

Why this is better than `FundingStage`:

- the PRD uses startup-style examples in places, but the product itself is broader than funded startups
- `OrganizationStageLabel` keeps the model flexible for agencies, local businesses, enterprise groups, and other non-startup targets
- if a specific mission cares about funding, that detail can still be captured in evidence or provider-specific enrichment later

#### 7. `TargetContact` (complex)

Purpose:

- stores the individual persona inside a target company

Suggested properties:

- `Guid Id`
- `string FullName`
- `string JobTitle`
- `string Department`
- `string Seniority`
- `bool IsDecisionMaker`
- `string? LinkedInUrl`
- `string? XHandle`
- `string? GitHubUsername`
- `string OpportunitySummary`
- `DateTime LastObservedAtUtc`
- `List<ContactChannel> ContactChannels`
- `List<EvidencePoint> EvidencePoints`

Why this class is necessary:

- the Anchor and Soul phases focus on people, not just companies

Why these properties matter:

- profile links align with the identity stitching story in the PRD
- `OpportunitySummary` gives a human-readable outreach hook
- `IsDecisionMaker` directly reflects the product’s persona targeting logic

#### 8. `ContactChannel` (complex)

Purpose:

- stores verified or discovered communication channels and identity links

Suggested properties:

- `Guid Id`
- `ContactChannelType Type`
- `string Value`
- `bool IsVerified`
- `DateTime? VerifiedAtUtc`
- `string Source`
- `decimal ConfidenceScore`

Why this class is necessary:

- the PRD repeatedly emphasizes verified contact data and multiple identity links

Why these properties matter:

- `IsVerified` is necessary because not every discovered channel is equally trustworthy
- `Source` makes the data auditable
- `ConfidenceScore` supports staged enrichment logic

#### 9. `EvidencePoint` (complex)

Purpose:

- stores raw proof used to justify scoring and outreach

Suggested properties:

- `Guid Id`
- `EvidenceKind Kind`
- `string Label`
- `string SourcePlatform`
- `string SourceUrl`
- `string Summary`
- `string RawSnippet`
- `DateTime CapturedAtUtc`
- `decimal ConfidenceScore`
- `bool IsQualificationSignal`

Why this class is necessary:

- it is the strongest domain link between the PRD and the lab
- Leadgen must prove where its insight came from

Why these properties matter:

- `SourceUrl` and `CapturedAtUtc` support auditability
- `RawSnippet` and `Summary` preserve both proof and explanation
- `Kind` drives broad analytics and filtering logic
- `Label` carries the product-specific meaning without forcing the enum to explode as Leadgen expands to more use cases

#### 10. `LeadDossier` (complex)

Purpose:

- represents the final lead package shown to the user

Suggested properties:

- `Guid Id`
- `Guid MissionRunId`
- `Guid TargetCompanyId`
- `Guid TargetContactId`
- `int LeadgenScore`
- `string SuggestedApproach`
- `string AdvantagePoint`
- `bool IsReadyForOutreach`
- `DateTime CreatedAtUtc`
- `DateTime LastUpdatedAtUtc`
- `int SupportingEvidenceCount`

Why this class is necessary:

- this is the product’s actual output, not the intermediate company/contact lists

Why these properties matter:

- `LeadgenScore` reflects the platform’s prioritization promise
- `SuggestedApproach` and `AdvantagePoint` reflect the PRD’s outreach hook logic
- `SupportingEvidenceCount` makes the score more explainable

### 7.3 Relationship design

Required relationships:

- `BusinessDnaMission` -> many `ClarificationQuestion`
- `BusinessDnaMission` -> many `MissionRun`
- `MissionRun` -> many `TargetCompany`
- `TargetCompany` -> many `TargetContact`
- `TargetContact` -> many `ContactChannel`
- `TargetContact` -> many `EvidencePoint`
- `MissionRun` <-> `SwarmAgent` through `MissionAgentAssignment`

Why these relationships are correct for the product:

- they mirror the actual Leadgen investigative flow
- they are not arbitrary lab-only relationships

Why the `N:N` is especially important:

- the product is agentic
- one run needs multiple agents
- one agent role/type can be reused across many runs

## 8. Supporting services that make the lab cleaner and more reusable

These are not required by the rubric, but they make the implementation cleaner and more future-proof.

### 8.1 `LeadgenSeedFactory`

Purpose:

- creates the seeded sample data for the three missions

Why it helps:

- keeps `Program.cs` readable
- future MVC or test code can reuse the same data-building logic

Why it relates to the product:

- lets you seed realistic Leadgen mission graphs rather than flat demo lists

### 8.2 `LeadgenQueryCatalog`

Purpose:

- stores the LINQ queries in named methods

Why it helps:

- makes each query explainable in isolation
- easier to demo and easier to modify during oral questioning

Why it relates to the product:

- these methods can evolve later into analytics or dashboard services

### 8.3 `MissionResearchSimulator`

Purpose:

- simulates parallel swarm steps with async methods

Why it helps:

- gives a clean location for `async/await` logic
- prevents `Program.cs` from becoming a long procedural script

Why it relates to the product:

- directly mirrors the product’s multi-agent execution idea

## 9. Seed data plan tied to product requirements

The lab requires at least three branched root objects. For Leadgen, the right root objects are **three Business DNA missions**.

### 9.1 Mission A. SQL optimization / database performance

Business DNA idea:

- product: cloud SQL optimization platform
- mechanic: identify and reduce database/query bottlenecks
- surface: API or web app
- persona: CTO / VP Engineering / Platform Lead
- villain: RDS latency, expensive inefficient queries
- delta: lower latency and lower infra spend

Why this belongs:

- it is close to the PRD examples about SQL optimization and pain signals
- it gives strong technical evidence examples from engineering channels

Suggested company/contact evidence types:

- GitHub issues about database scaling
- X posts about latency or cloud costs
- hiring signals for platform/database engineers

### 9.2 Mission B. Support QA automation

Business DNA idea:

- product: AI quality assurance for customer support calls or tickets
- mechanic: automate review and score support interactions
- surface: SaaS platform
- persona: Head of Support / QA Manager / Operations Lead
- villain: manual call review and spreadsheet tracking
- delta: faster QA coverage and less manager time

Why this belongs:

- it produces a less technical but still very realistic B2B use case
- it broadens the domain so queries are not biased toward only engineering leads

Suggested company/contact evidence types:

- job posts about support scaling
- operations leaders discussing QA bottlenecks
- evidence of growing support teams

### 9.3 Mission C. Corporate venue booking engine

Business DNA idea:

- product: booking workflow platform for corporate venues and vendors
- mechanic: centralize booking and coordination
- surface: web app
- persona: Operations Director / Venue Manager
- villain: fragmented email chains and spreadsheets
- delta: faster booking turnaround and fewer errors

Why this belongs:

- it maps directly to one of the high-density examples in the documents
- it proves the model supports non-SaaS-ops verticals too

Suggested company/contact evidence types:

- venue groups with multi-location operations
- expansion announcements
- operations complaints about manual coordination

### 9.4 Branching depth per mission

Each mission should include at minimum:

- 2-3 clarification questions
- 1 mission run
- 3 agent assignments
- 3 target companies
- 2 contacts per company
- 2-3 contact channels per contact
- 2-4 evidence points for strong contacts
- 1 dossier for the best contact per company

Why this amount is right:

- enough structure for rich LINQ queries
- enough branching to satisfy the rubric
- still manageable for a lab submission

## 10. LINQ query plan with product meaning

The LINQ queries should answer real Leadgen questions.

### Query 1. Missions below confidence threshold

Business question:

- which missions are not ready to launch because the Business DNA is too vague?

Suggested operators:

- `Where`
- `OrderBy`
- `Select`

Why this matters to Leadgen:

- it directly supports Phase 1 gatekeeping
- it prevents wasted research runs

### Query 2. Unanswered clarification questions by slot

Business question:

- which ontology slots most often block readiness across missions?

Suggested operators:

- `SelectMany`
- `Where`
- `GroupBy`
- `Count`

Why this matters to Leadgen:

- it reveals where user input is weakest
- later it could inform UI copy or smarter follow-up prompts

### Query 3. Agent workload by role

Business question:

- which agent roles are most heavily assigned across mission runs?

Suggested operators:

- `SelectMany`
- `GroupBy`
- `Count`
- `OrderByDescending`

Why this matters to Leadgen:

- it mirrors swarm capacity planning
- later it could drive scheduler logic or telemetry

### Query 4. Best-fit companies

Business question:

- which companies are most aligned with the mission and have verified HQ data?

Suggested operators:

- `Where`
- `OrderByDescending`
- `Take`

Why this matters to Leadgen:

- shortlisting is a core product task
- verified company evidence should outrank weak matches

### Query 5. Outreach-ready contacts

Business question:

- which contacts have both high-value qualification evidence and at least one verified work channel?

Suggested operators:

- `SelectMany`
- `Where`
- `Any`

Why this matters to Leadgen:

- this is close to the actual handoff point from research to sales outreach

### Query 6. Highest scoring dossier per mission

Business question:

- what is the best lead dossier generated from each mission?

Suggested operators:

- `GroupBy`
- `OrderByDescending`
- `First`

Why this matters to Leadgen:

- the product promises prioritization, not just bulk output

### Query 7. Evidence distribution by kind, label, and platform

Business question:

- what types of evidence are we actually finding, and on which platforms?

Suggested operators:

- `SelectMany`
- `GroupBy`
- `Select`

Why this matters to Leadgen:

- shows whether the swarm is discovering the right mix of signals, verifications, organization evidence, and profile evidence
- later helps tune sourcing strategy

### Query 8. High-score leads missing key contact channels

Business question:

- which leads look strong but still need enrichment?

Suggested operators:

- `Where`
- `Any`
- `Contains`

Why this matters to Leadgen:

- identifies when the Stitcher/contact enrichment phase is still necessary

### Query 9. Recent signals in the last 30 days

Business question:

- which leads have the freshest signals and should be prioritized now?

Suggested operators:

- `Where`
- `OrderByDescending`

Why this matters to Leadgen:

- the PRD positions the product as real-time intelligence, not stale data

### Query 10. Average lead score by mission type or surface

Business question:

- which kinds of missions generate stronger leads?

Suggested operators:

- `GroupBy`
- `Average`
- `Select`

Why this matters to Leadgen:

- later this becomes product analytics and helps improve onboarding and scoring

## 11. Async/await plan with direct product alignment

### 11.1 What to implement

Create a `MissionResearchSimulator` with methods such as:

- `Task<List<TargetCompany>> RunScoutAsync(BusinessDnaMission mission, CancellationToken cancellationToken = default)`
- `Task<List<TargetCompany>> RunSentinelAsync(BusinessDnaMission mission, CancellationToken cancellationToken = default)`
- `Task<List<TargetContact>> RunAnchorAsync(IEnumerable<TargetCompany> companies, CancellationToken cancellationToken = default)`
- `Task<List<EvidencePoint>> RunSoulAsync(IEnumerable<TargetContact> contacts, CancellationToken cancellationToken = default)`

These methods can use:

- `Task.Delay(...)`
- seeded in-memory data
- predictable fake outputs

### 11.2 Why this is the right async design

Because it mirrors the PRD:

- `Scout` and `Sentinel` can run in parallel
- `Anchor` depends on company outputs
- `Soul` depends on contact outputs
- later `Stitcher` could depend on evidence/contact readiness

### 11.3 What `Program.cs` should demonstrate

The console runner should:

1. load three missions
2. print a short summary
3. execute the LINQ queries
4. pick one mission and simulate a run
5. run `Scout` and `Sentinel` with `Task.WhenAll(...)`
6. continue to `Anchor`
7. continue to `Soul`
8. print the resulting mission summary and maybe a sample dossier

### 11.4 What to be able to explain orally

Be able to explain:

- what `Task` is
- why async methods return `Task` or `Task<T>`
- why `await` prevents blocking compared to `.Wait()` or `.Result`
- why `Task.WhenAll(...)` fits the swarm concept
- why cancellation and `try/catch` matter in long-running work

## 12. What not to implement in Lab 1 and why

Do not implement these in Lab 1:

- real API integrations
- SignalR hubs
- database persistence
- authentication
- scraping
- Microsoft Agent Framework integration
- real background workers inside MVC

Why not:

- they are valid future product concerns
- they are not needed to prove object modeling, LINQ, and async understanding
- they would consume time without improving the Lab 1 grading criteria

The lab should build the **semantic foundation** of the product, not the full infrastructure.

## 13. Detailed implementation order with rationale

### Step 1. Keep the existing MVC app as the future product host

Action:

- do not replace the web project
- leave controllers/views mostly untouched for Lab 1

Why:

- the MVC app is the future container for the real product
- the lab should add foundations, not derail the app structure

### Step 2. Create `lab-1/ai-agent-log.md`

Action:

- add the log file in the required folder

Why:

- directly required by the lab brief
- keeps the deliverable complete

### Step 3. Add `Leadgen.Model`

Action:

- create a class library for enums and entities

Why:

- gives a reusable domain layer
- maps to the course guidance about layered applications
- supports both MVC and console runner

### Step 4. Implement enums first

Action:

- create all domain enums before entity classes

Why:

- statuses, agent roles, contact channel types, and generic evidence kinds are central vocabulary
- they make the truly closed parts of the model clearer and safer than free-text strings

### Step 5. Implement domain classes with collection initialization

Action:

- create the 10 recommended classes
- initialize list properties in constructors

Why:

- collection-based relationships are everywhere in Leadgen
- constructor initialization avoids null-reference problems

### Step 6. Add `Leadgen.Lab1Runner`

Action:

- create the console app
- add a project reference to `Leadgen.Model`

Why:

- this is the cleanest way to satisfy the “main program” requirement

### Step 7. Add `LeadgenSeedFactory`

Action:

- create the three root missions and full object graphs

Why:

- keeps sample data realistic and reusable
- prevents `Program.cs` from becoming a giant static configuration blob

### Step 8. Add `LeadgenQueryCatalog`

Action:

- implement the 10 recommended LINQ queries as methods

Why:

- improves explainability during oral questioning
- keeps the console app focused on output flow

### Step 9. Add `MissionResearchSimulator`

Action:

- implement the async demo using staged fake agent methods

Why:

- demonstrates understanding of `async/await`
- directly mirrors the future product architecture

### Step 10. Print concise console output

Action:

- print mission summaries and query results in a readable format

Why:

- the assessor needs to see that the model and queries actually work
- readable output also helps you explain the logic

### Step 11. Validate against both rubric and PRD

Action:

- check the rubric
- check the product traceability matrix

Why:

- a technically correct lab can still be strategically weak if it does not reflect Leadgen

## 14. Acceptance checklist

Lab 1 is complete only when all of the following are true:

- at least 7 classes exist
- at least 4 classes have more than 5 meaningful properties
- at least one enum exists
- at least one `DateTime` exists
- `1:N` and `N:N` relationships are implemented clearly
- 3 branched root mission objects are seeded
- the root objects are Leadgen missions, not generic demo entities
- LINQ queries answer real Leadgen questions
- async demo mirrors the future swarm flow
- `lab-1/ai-agent-log.md` exists
- all assessed code is committed to GitHub `main`

## 15. Future carry-forward value after Lab 1

If implemented this way, Lab 1 becomes the foundation for later work:

- `BusinessDnaMission` can later become the input model for the MVC intake flow
- `ClarificationQuestion` can later power the UI for mission refinement
- `MissionRun` and `SwarmAgent` can later be used in hosted/background execution
- `TargetCompany`, `TargetContact`, `ContactChannel`, and `EvidencePoint` can later map to persistence
- `LeadDossier` can later feed the dashboard and archive
- LINQ queries can later evolve into service-layer analytics
- `MissionResearchSimulator` can later be replaced by real background orchestration and external provider calls

That is the main reason to implement Lab 1 this way: it is not disposable coursework. It becomes the first real slice of Leadgen.

## 16. Recommended official research references

These are the main official references that support the technical decisions in this plan:

- ASP.NET Core MVC overview  
  https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-9.0
- `dotnet reference add` for project-to-project references  
  https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-reference-add
- LINQ overview / introduction  
  https://learn.microsoft.com/en-us/dotnet/csharp/linq/
- `async` keyword reference  
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async
- Asynchronous programming with async and await  
  https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
- Background tasks with hosted services in ASP.NET Core  
  https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-10.0
- ASP.NET Core SignalR overview  
  https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0
- Microsoft Agent Framework overview  
  https://learn.microsoft.com/en-gb/agent-framework/overview/agent-framework-overview

## 17. Final recommendation

The best Lab 1 implementation for this repository is:

- **domain-first**
- **mission-centered**
- **evidence-aware**
- **LINQ-driven**
- **async-simulated**
- **architecturally reusable**

That is the version of Lab 1 that stays faithful to the Leadgen PRDs and creates assets that will still matter when you start implementing the real product in ASP.NET MVC.
