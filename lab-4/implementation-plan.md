# Leadgen Lab 4 Implementation Plan

## Source Requirements

This plan is based on `lab-4/Lab4 copy.md` and the current Leadgen MVC codebase after Lab 3.

Lab 4 requires:

- complete CRUD support where business rules allow it
- AJAX search on every list page
- an AJAX autocomplete dropdown for related records
- client-side and server-side validation
- advanced JavaScript that supports the application
- a reusable date-time control built as a partial view, not a browser-native date picker
- support for Croatian and English date display expectations

## Existing Baseline

Before Lab 4, Leadgen already had:

- EF Core persistence with SQLite
- entity list and details pages
- one create/edit/delete flow for clarification questions
- repository-backed read pages
- semantic routing for the main entities
- a dossier-style UI and existing JavaScript animation on the mission canvas

The gap was that most entities were still read-only, list pages did not perform AJAX search, relationship selection used static inputs/selects, and date input was not centralized through a reusable control.

## Implementation Strategy

### CRUD

CRUD is implemented through the existing entity controllers:

- `MissionsController`
- `ClarificationQuestionsController`
- `MissionRunsController`
- `MissionAgentAssignmentsController`
- `SwarmAgentsController`
- `TargetCompaniesController`
- `TargetContactsController`
- `ContactChannelsController`
- `EvidencePointsController`
- `LeadDossiersController`

Each controller now exposes:

- `Index`
- `Details`
- `Create` GET/POST
- `Edit` GET/POST
- `Delete` GET/POST

Deletion is explicit about dependent records. For example, deleting a company removes linked dossiers first, then lets EF cascade company-owned contacts, channels, and evidence.

### AJAX Search

All list pages use:

- shared partial: `Views/Shared/_AjaxSearch.cshtml`
- endpoint: `EntitySearchController.Search`
- client behavior: `initAjaxSearch()` in `wwwroot/js/site.js`

The endpoint returns JSON rows/cards, and JavaScript refreshes only the table body or card grid. This avoids full-page reloads and satisfies the AJAX search requirement consistently.

### Autocomplete Dropdown

All relationship selectors use:

- shared partial: `Views/Shared/_AutocompleteSelect.cshtml`
- endpoint: `LookupsController`
- client behavior: `initAutocomplete()` in `wwwroot/js/site.js`

The control behaves like a dropdown but fetches options asynchronously from the server. It is used for missions, runs, agents, companies, and contacts.

### Validation

Server-side validation is done in controller POST actions:

- selected foreign keys must exist
- date ranges must be coherent
- unique values such as run code and agent code name must stay unique
- relationship rules such as dossier contact belonging to selected company are checked

Client-side validation is handled through:

- MVC unobtrusive validation scripts loaded from `_Layout.cshtml`
- blur-triggered validation in `site.js`
- custom autocomplete/date messages for controls that are not ordinary `asp-for` text inputs

### Custom Date-Time Control

Date-time fields use:

- shared partial: `Views/Shared/_DateTimeControl.cshtml`
- client behavior: `initDateControls()` in `wwwroot/js/site.js`

The visible input formats dates as:

- `dd.MM.yyyy HH:mm` for Croatian browser settings
- `MM/dd/yyyy HH:mm` for English browser settings

The posted value is stored in a hidden ISO-style field so model binding stays stable on the server.

### Advanced JavaScript

Lab 4 JavaScript is intentionally tied to app behavior:

- debounced AJAX searches
- animated result refreshes
- autocomplete dropdown state management
- custom date picker calendar rendering
- blur-triggered form validation
- existing mission-canvas progress animation remains in place

## Verification Plan

Required verification:

- `dotnet build leadgen.sln --no-restore`
- run the app locally
- open representative list and form pages
- call representative `/search/{entity}` endpoints
- call representative `/lookups/{entity}` endpoints
- check git ignored state for Lab 3 and Lab 4 study guide files
