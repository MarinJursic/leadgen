---
name: leadgen-ef
description: Use when changing the Leadgen EF Core model, DbContext, migrations, or startup seeding.
---

# Leadgen EF

## Purpose

This skill is for Entity Framework work inside the Leadgen MVC app.

Use it when the task involves:

- adding or changing entity foreign keys
- updating navigation properties
- editing `Data/LeadgenDbContext.cs`
- creating or applying migrations
- adjusting the startup seed path in `Data/LeadgenDbSeeder.cs`
- replacing read logic that used the old mock repository

## Repo rules

- Preserve the existing Leadgen domain vocabulary.
- Keep `BusinessDnaMission` as the root aggregate.
- Keep `SurfaceTags` stored through the existing JSON conversion unless there is a strong reason to normalize it.
- Prefer explicit relationships in Fluent API for anything non-trivial.
- Preserve the sample dataset by reusing `LeadgenSeedFactory`.

## Expected workflow

1. Inspect the affected entity plus `LeadgenDbContext`.
2. Confirm whether the change is scalar-only or changes relationships.
3. Update the entity annotations only where they add clarity.
4. Put relationship, precision, and conversion rules in `LeadgenDbContext`.
5. Build before generating a migration.
6. Generate a migration only after the model compiles cleanly.
7. Run the app or `database update` to verify schema and seed behavior.

## Verification

- `dotnet build leadgen.sln`
- `dotnet ef migrations add <Name>`
- `dotnet ef database update`
- open at least one affected list/details page
