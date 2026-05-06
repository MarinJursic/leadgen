---
name: leadgen-list-page
description: Use when adding a new list/index page to Leadgen while preserving the dossier-style MVC UI.
---

# Leadgen List Page

## Purpose

This skill is for adding a new read-oriented list page to the Leadgen app.

Use it when the task requires:

- a new controller `Index` page
- a new query or projection over EF data
- a new Razor list/table/card view
- a new navigation entry in the shared layout or immersive vault

## Repo rules

- Keep the visual language white-first, restrained, and dossier-led.
- Prefer semantic URLs for new non-trivial pages.
- Use projection view models for non-entity lists.
- Avoid generic scaffold markup.

## Expected workflow

1. Define the user-facing purpose of the page.
2. Decide whether the page is entity-backed or projection-backed.
3. Create a dedicated view model if the page joins multiple entities.
4. Add the controller action and route.
5. Build the Razor page using existing `surface-panel`, `detail-panel`, and `button-link` patterns.
6. Add a navigation path from `_Layout.cshtml` or the immersive home/mission vault.

## Preferred pattern in this repo

- Controller: thin, query-focused
- Query source: `LeadgenDbContext` or EF-backed repository
- View: editorial cards or integrated tables
- URL: semantic, short, and tied to the feature name
