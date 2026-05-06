---
name: leadgen-edit-form
description: Use when adding create/edit MVC forms in Leadgen with tag helpers, validation, and shared partials.
---

# Leadgen Edit Form

## Purpose

This skill is for create/edit form flows in the Leadgen MVC app.

Use it when the task requires:

- a GET form page
- a POST handler
- model binding and validation
- a reusable `_Form.cshtml` partial

## Repo rules

- Prefer a dedicated form view model over binding entities directly.
- Reuse one partial for both create and edit where practical.
- Use tag helpers: `asp-for`, `asp-action`, `asp-route-*`, `asp-validation-for`.
- Keep form styling aligned with the existing dossier UI.
- Do not add unnecessary CRUD breadth; add the smallest useful form flow first.

## Expected workflow

1. Pick the simplest real entity that demonstrates the relationship being tested.
2. Create a form view model with validation attributes.
3. Build lookup/select-list data in the controller.
4. Add GET and POST actions with antiforgery validation.
5. Extract shared fields into `_Form.cshtml`.
6. Repopulate lookup data when returning validation errors.
7. Verify create, edit, and redirect flows manually.

## Good fit in this repo

- `ClarificationQuestion`
- `ContactChannel`
- other small child entities with one obvious parent relationship
