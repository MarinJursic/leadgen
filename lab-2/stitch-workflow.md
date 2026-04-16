# Leadgen Stitch Workflow

## Current state

- the repo has a dedicated UX sub-agent prompt
- the MVC implementation can proceed without live Stitch access
- live Stitch connection is intentionally deferred until the local MCP setup is finished

## Remaining prerequisite

Live Stitch MCP usage still depends on a working local Stitch plugin setup and a valid API key.

Until that is finished, the repo can still proceed with:

- design-system drafting
- prompt drafting
- MVC implementation
- UI structure work

## Intended usage order

1. `taste-design`
   Set the premium monochrome taste rules.
2. `design-md`
   Generate or refine `.stitch/DESIGN.md`.
3. `enhance-prompt`
   Turn rough page asks into cleaner Stitch prompts.
4. `stitch-design`
   Generate or iterate on page-level screen designs.

## MVC translation rule

Stitch is the UX ideation and consistency layer.

The final deliverable remains:

- ASP.NET Core MVC views
- static mock repository data
- strongly typed page models

Stitch should shape the interface, not replace the application structure.
