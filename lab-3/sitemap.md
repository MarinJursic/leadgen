# Leadgen Sitemap

## Routing Baseline

The application keeps the default conventional MVC route in `Program.cs`:

```csharp
{controller=Home}/{action=Index}/{id?}
```

On top of that, Lab 3 adds attribute-routed semantic URLs for the main mission, company, contact, dossier, question, and outreach flows.

## Page Map

| URL | Route type | Controller | Action | Parameters | View |
| --- | --- | --- | --- | --- | --- |
| `/` | Attribute | `HomeController` | `Index` | none | `Views/Home/Index.cshtml` |
| `/mission-control` | Attribute | `HomeController` | `Mission` | optional query string `dna` | `Views/Home/Mission.cshtml` |
| `/Home/Privacy` | Conventional | `HomeController` | `Privacy` | none | `Views/Home/Privacy.cshtml` |
| `/Home/Error` | Conventional | `HomeController` | `Error` | none | `Views/Shared/Error.cshtml` |
| `/missions` | Attribute | `MissionsController` | `Index` | none | `Views/Missions/Index.cshtml` |
| `/missions/{id:guid}` | Attribute | `MissionsController` | `Details` | `id` | `Views/Missions/Details.cshtml` |
| `/questions` | Attribute | `ClarificationQuestionsController` | `Index` | none | `Views/ClarificationQuestions/Index.cshtml` |
| `/questions/{id:guid}` | Attribute | `ClarificationQuestionsController` | `Details` | `id` | `Views/ClarificationQuestions/Details.cshtml` |
| `/questions/new` | Attribute | `ClarificationQuestionsController` | `Create` (GET) | none | `Views/ClarificationQuestions/Create.cshtml` |
| `/questions/new` | Attribute | `ClarificationQuestionsController` | `Create` (POST) | form body | `Views/ClarificationQuestions/Create.cshtml` |
| `/questions/{id:guid}/edit` | Attribute | `ClarificationQuestionsController` | `Edit` (GET) | `id` | `Views/ClarificationQuestions/Edit.cshtml` |
| `/questions/{id:guid}/edit` | Attribute | `ClarificationQuestionsController` | `Edit` (POST) | `id` + form body | `Views/ClarificationQuestions/Edit.cshtml` |
| `/questions/{id:guid}/delete` | Attribute | `ClarificationQuestionsController` | `Delete` (GET) | `id` | `Views/ClarificationQuestions/Delete.cshtml` |
| `/questions/{id:guid}/delete` | Attribute | `ClarificationQuestionsController` | `Delete` (POST) | `id` + form body | `Views/ClarificationQuestions/Delete.cshtml` |
| `/MissionRuns` | Conventional | `MissionRunsController` | `Index` | none | `Views/MissionRuns/Index.cshtml` |
| `/MissionRuns/Details/{id}` | Conventional | `MissionRunsController` | `Details` | `id` | `Views/MissionRuns/Details.cshtml` |
| `/MissionAgentAssignments` | Conventional | `MissionAgentAssignmentsController` | `Index` | none | `Views/MissionAgentAssignments/Index.cshtml` |
| `/MissionAgentAssignments/Details/{id}` | Conventional | `MissionAgentAssignmentsController` | `Details` | `id` | `Views/MissionAgentAssignments/Details.cshtml` |
| `/SwarmAgents` | Conventional | `SwarmAgentsController` | `Index` | none | `Views/SwarmAgents/Index.cshtml` |
| `/SwarmAgents/Details/{id}` | Conventional | `SwarmAgentsController` | `Details` | `id` | `Views/SwarmAgents/Details.cshtml` |
| `/companies` | Attribute | `TargetCompaniesController` | `Index` | none | `Views/TargetCompanies/Index.cshtml` |
| `/companies/{id:guid}` | Attribute | `TargetCompaniesController` | `Details` | `id` | `Views/TargetCompanies/Details.cshtml` |
| `/contacts` | Attribute | `TargetContactsController` | `Index` | none | `Views/TargetContacts/Index.cshtml` |
| `/contacts/{id:guid}` | Attribute | `TargetContactsController` | `Details` | `id` | `Views/TargetContacts/Details.cshtml` |
| `/ContactChannels` | Conventional | `ContactChannelsController` | `Index` | none | `Views/ContactChannels/Index.cshtml` |
| `/ContactChannels/Details/{id}` | Conventional | `ContactChannelsController` | `Details` | `id` | `Views/ContactChannels/Details.cshtml` |
| `/EvidencePoints` | Conventional | `EvidencePointsController` | `Index` | none | `Views/EvidencePoints/Index.cshtml` |
| `/EvidencePoints/Details/{id}` | Conventional | `EvidencePointsController` | `Details` | `id` | `Views/EvidencePoints/Details.cshtml` |
| `/dossiers` | Attribute | `LeadDossiersController` | `Index` | none | `Views/LeadDossiers/Index.cshtml` |
| `/dossiers/{id:guid}` | Attribute | `LeadDossiersController` | `Details` | `id` | `Views/LeadDossiers/Details.cshtml` |
| `/outreach/queue` | Attribute | `OutreachQueueController` | `Index` | none | `Views/OutreachQueue/Index.cshtml` |

## Notes

- The semantic URLs are the intended canonical URLs for Lab 3.
- The question create/edit/delete pages demonstrate custom routing plus form handling.
- The outreach queue page is the extra Lab 3 list page added on top of the original entity browsing surface.
