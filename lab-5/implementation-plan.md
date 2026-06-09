# Lab 5 Implementation Plan

## Research Summary

`Lab5.md` asks for a secured API layer over the existing MVC application:

- complete DTO-based CRUD API support for every entity
- local account authentication with ASP.NET Core Identity
- authorization using `Admin` plus at least one more role
- asynchronous file upload tied to a concrete record
- one external authentication provider, using Google or Facebook
- integration tests for API CRUD endpoints, including success, missing IDs, and validation errors

The assignment examples use quizzes. Leadgen has no quiz entity, so the matching domain concept is `BusinessDnaMission`. Lab 5 upload support will therefore attach supporting documents to missions.

Microsoft's ASP.NET Core docs confirm the implementation direction:

- `[ApiController]` plus action result helpers such as `CreatedAtAction` are the standard API pattern.
- Identity supports `AddDefaultIdentity`, EF stores, customized user types, and role registration.
- Google login should read client ID and secret from configuration or user secrets, not hardcoded source.
- Upload handlers should use `multipart/form-data`, `IFormFile`, a generated storage filename, and persisted metadata.
- Integration tests should use `WebApplicationFactory` with isolated test configuration and database setup.

## Entity Coverage

The API will cover the active Leadgen EF entities:

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
- `MissionAttachment` for the new upload feature

## Implementation Steps

1. Add Identity packages and configure `LeadgenDbContext` to inherit from `IdentityDbContext<AppUser>`.
2. Add `AppUser` with required `OIB` and `JMBG` fields.
3. Seed `Admin` and `Manager` roles plus deterministic development accounts for local verification.
4. Add Razor Identity endpoints for login, register, logout, external login confirmation, and access denied.
5. Add `_LoginPartial` to the shared layout.
6. Wire Google authentication through `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret`.
7. Apply MVC authorization rules:
   - index/search pages are anonymous
   - details pages require authentication
   - create/edit require `Admin` or `Manager`
   - delete requires `Admin`
8. Add DTO/request models and mapper helpers under `Models/Api`.
9. Add API controllers under `Controllers/Api` with:
   - `GET /api/{entity}?query=...`
   - `GET /api/{entity}/{id}`
   - `POST /api/{entity}`
   - `PUT /api/{entity}/{id}`
   - `DELETE /api/{entity}/{id}`
10. Keep API responses DTO-only and use nested summary DTOs for related data where useful.
11. Add `MissionAttachment` and relationship configuration.
12. Add mission edit upload UI:
   - async upload form
   - AJAX attachment list
   - delete button per attachment
13. Add a migration for Identity and mission attachments.
14. Add an integration test project with `WebApplicationFactory`, isolated SQLite databases, authenticated test clients, and CRUD/error/validation tests for all API controllers.
15. Run restore, build, and tests.
16. Commit and push the Lab 5 work without touching unrelated user-modified Lab 2 hook logs.

## Verification Checklist

- `dotnet build leadgen.sln`
- `dotnet test`
- Manual smoke check for mission attachment endpoints if the app starts cleanly
- `git status --short`
- commit and push to GitHub
