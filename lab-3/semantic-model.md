# Leadgen Semantic Model

## Overview

Leadgen now uses EF Core with SQLite and a relational schema that preserves the original Lab 1 and Lab 2 business graph:

1. `BusinessDnaMission` is the root intake entity.
2. each mission can have many clarification questions and many research runs.
3. each run can have many agent assignments, target companies, and lead dossiers.
4. each company can have many contacts.
5. each contact can have many contact channels and evidence points.
6. each dossier links one run, one company, and one primary contact.

## Tables

| Table | PK | Main properties | Notes |
| --- | --- | --- | --- |
| `BusinessDnaMissions` | `Id` | `MissionName`, `ProductName`, `Mechanic`, `PrimarySurface`, `SurfaceTags`, `Persona`, `Villain`, `Delta`, `ConfidenceScore`, `CreatedAtUtc`, `Status` | Root mission/intake table. `SurfaceTags` is stored as JSON text through an EF value converter. |
| `ClarificationQuestions` | `Id` | `BusinessDnaMissionId`, `SlotName`, `Prompt`, `Reason`, `IsAnswered`, `Answer`, `CreatedAtUtc`, `AnsweredAtUtc` | Child table of `BusinessDnaMissions`. |
| `MissionRuns` | `Id` | `BusinessDnaMissionId`, `RunCode`, `StartedAtUtc`, `CompletedAtUtc`, `Status`, `SearchRegion`, `TokenBudget`, `EstimatedCostUsd` | Execution instance for a mission. `RunCode` is unique. |
| `SwarmAgents` | `Id` | `CodeName`, `Role`, `Provider`, `Temperature`, `MaxConcurrentTasks`, `IsActive`, `LastHeartbeatUtc`, `CurrentFocus` | Agent catalog. `CodeName` is unique. |
| `MissionAgentAssignments` | `Id` | `MissionRunId`, `SwarmAgentId`, `AssignedAtUtc`, `Responsibility`, `TokenBudget`, `Status` | Join/entity table between runs and agents. |
| `TargetCompanies` | `Id` | `MissionRunId`, `Name`, `Domain`, `Industry`, `HeadquartersCity`, `HeadquartersCountry`, `OrganizationStageLabel`, `LastSignalAtUtc`, `EmployeeCount`, `IsHeadquartersVerified`, `MatchScore` | Company discovered during a run. |
| `TargetContacts` | `Id` | `TargetCompanyId`, `FullName`, `JobTitle`, `Department`, `Seniority`, `IsDecisionMaker`, `LinkedInUrl`, `XHandle`, `GitHubUsername`, `OpportunitySummary`, `LastObservedAtUtc` | Contact/person linked to one company. |
| `ContactChannels` | `Id` | `TargetContactId`, `Type`, `Value`, `IsVerified`, `VerifiedAtUtc`, `Source`, `ConfidenceScore` | Contact vectors for a person. |
| `EvidencePoints` | `Id` | `TargetContactId`, `Kind`, `Label`, `SourcePlatform`, `SourceUrl`, `Summary`, `RawSnippet`, `CapturedAtUtc`, `ConfidenceScore`, `IsQualificationSignal` | Evidence and proof linked to one contact. |
| `LeadDossiers` | `Id` | `MissionRunId`, `TargetCompanyId`, `TargetContactId`, `LeadgenScore`, `SuggestedApproach`, `AdvantagePoint`, `IsReadyForOutreach`, `CreatedAtUtc`, `LastUpdatedAtUtc`, `SupportingEvidenceCount` | Final lead output row linking run + company + contact. |

## Relationships

| From | Relationship | To | FK |
| --- | --- | --- | --- |
| `BusinessDnaMission` | `1 -> N` | `ClarificationQuestion` | `ClarificationQuestions.BusinessDnaMissionId` |
| `BusinessDnaMission` | `1 -> N` | `MissionRun` | `MissionRuns.BusinessDnaMissionId` |
| `MissionRun` | `1 -> N` | `MissionAgentAssignment` | `MissionAgentAssignments.MissionRunId` |
| `SwarmAgent` | `1 -> N` | `MissionAgentAssignment` | `MissionAgentAssignments.SwarmAgentId` |
| `MissionRun` | `1 -> N` | `TargetCompany` | `TargetCompanies.MissionRunId` |
| `TargetCompany` | `1 -> N` | `TargetContact` | `TargetContacts.TargetCompanyId` |
| `TargetContact` | `1 -> N` | `ContactChannel` | `ContactChannels.TargetContactId` |
| `TargetContact` | `1 -> N` | `EvidencePoint` | `EvidencePoints.TargetContactId` |
| `MissionRun` | `1 -> N` | `LeadDossier` | `LeadDossiers.MissionRunId` |
| `TargetCompany` | `1 -> N` | `LeadDossier` | `LeadDossiers.TargetCompanyId` |
| `TargetContact` | `1 -> N` | `LeadDossier` | `LeadDossiers.TargetContactId` |

## Navigation Summary

### Mission graph

- `BusinessDnaMission.Runs`
- `BusinessDnaMission.ClarificationQuestions`
- `MissionRun.Mission`

### Execution graph

- `MissionRun.AgentAssignments`
- `MissionRun.TargetCompanies`
- `MissionRun.LeadDossiers`
- `MissionAgentAssignment.MissionRun`
- `MissionAgentAssignment.SwarmAgent`
- `SwarmAgent.MissionAssignments`

### Prospect graph

- `TargetCompany.MissionRun`
- `TargetCompany.Contacts`
- `TargetContact.TargetCompany`
- `TargetContact.ContactChannels`
- `TargetContact.EvidencePoints`

### Dossier graph

- `LeadDossier.MissionRun`
- `LeadDossier.TargetCompany`
- `LeadDossier.TargetContact`

## Storage Notes

- All primary keys are `Guid`.
- Enums are stored as integers.
- Decimal scores and costs are configured with explicit precision in `LeadgenDbContext`.
- `SurfaceTags` stays as `List<string>` in C#, but is persisted as JSON text in the database so the original business DNA model stays intact.
