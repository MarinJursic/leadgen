# Lab 4 CRUD Matrix

| Entity | List | AJAX search | Create | Edit | Delete | Relationship autocomplete | Date partial usage |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `BusinessDnaMission` | `/missions` | `/search/missions` | `/missions/new` | `/missions/{id}/edit` | `/missions/{id}/delete` | n/a | `CreatedAtUtc` |
| `ClarificationQuestion` | `/questions` | `/search/questions` | `/questions/new` | `/questions/{id}/edit` | `/questions/{id}/delete` | Mission | `CreatedAtUtc`, `AnsweredAtUtc` |
| `MissionRun` | `/runs` | `/search/runs` | `/runs/new` | `/runs/{id}/edit` | `/runs/{id}/delete` | Mission | `StartedAtUtc`, `CompletedAtUtc` |
| `MissionAgentAssignment` | `/assignments` | `/search/assignments` | `/assignments/new` | `/assignments/{id}/edit` | `/assignments/{id}/delete` | Run, Agent | `AssignedAtUtc` |
| `SwarmAgent` | `/agents` | `/search/agents` | `/agents/new` | `/agents/{id}/edit` | `/agents/{id}/delete` | n/a | `LastHeartbeatUtc` |
| `TargetCompany` | `/companies` | `/search/companies` | `/companies/new` | `/companies/{id}/edit` | `/companies/{id}/delete` | Run | `LastSignalAtUtc` |
| `TargetContact` | `/contacts` | `/search/contacts` | `/contacts/new` | `/contacts/{id}/edit` | `/contacts/{id}/delete` | Company | `LastObservedAtUtc` |
| `ContactChannel` | `/channels` | `/search/channels` | `/channels/new` | `/channels/{id}/edit` | `/channels/{id}/delete` | Contact | `VerifiedAtUtc` |
| `EvidencePoint` | `/evidence` | `/search/evidence` | `/evidence/new` | `/evidence/{id}/edit` | `/evidence/{id}/delete` | Contact | `CapturedAtUtc` |
| `LeadDossier` | `/dossiers` | `/search/dossiers` | `/dossiers/new` | `/dossiers/{id}/edit` | `/dossiers/{id}/delete` | Run, Company, Contact | `CreatedAtUtc`, `LastUpdatedAtUtc` |
| Outreach queue | `/outreach/queue` | `/search/queue` | n/a | n/a | n/a | n/a | n/a |

## Delete Rules

- Mission delete removes its full mission graph.
- Run delete removes linked dossiers first, then the run graph.
- Agent delete removes assignment links first.
- Company delete removes linked dossiers first, then company-owned contacts and their child data.
- Contact delete removes linked dossiers first, then channels and evidence.
- Channel, evidence, assignment, question, and dossier delete remove only the selected record.
