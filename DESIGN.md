# Design System: Leadgen Intelligence Swarm
**Project ID:** 5382043755968258888

## 1. Visual Theme & Atmosphere
Leadgen should feel like a precise B2B research command desk: quiet, editorial, dense enough for repeated work, and polished without looking like a marketing landing page. The interface uses a white-first canvas, dark ink text, thin borders, low-radius geometry, and measured spacing to make missions, evidence, uploads, and queue items easy to scan.

The app should avoid bright gradients, decorative blobs, oversized cards, and default Bootstrap styling. Visual emphasis comes from typography, hierarchy, tables, compact action rows, and small status markers.

## 2. Color Palette & Roles
* **Paper Canvas (#F3F1EA):** Main app background. Warm enough to distinguish the workspace from pure white panels.
* **White Ledger (#FFFEFA):** Primary surfaces, panels, cards, headers, forms, and menus.
* **Carbon Ink (#111111):** Primary text, active navigation, primary buttons, selected date cells.
* **Soft Graphite (#5F625C):** Secondary text, metadata, helper copy, inactive navigation.
* **Fine Rule (#DEDAD0):** Default borders and table dividers.
* **Heavy Rule (#C6C1B6):** Stronger borders for inputs, focused regions, and important separators.
* **Signal Teal (#0F766E):** Positive or active system accents used sparingly for focus, live states, and completed work.
* **Amber Queue (#B7791F):** Pending or queued state accent.
* **Danger Red (#8F1D1D):** Validation, destructive actions, and deletion warnings.

## 3. Typography Rules
Use **Inter** for body copy and dense interface text. Use **Space Grotesk** for display headings and page titles. Use **IBM Plex Mono** for labels, metadata, status chips, and small command text.

Headings are bold and tight, but not exaggerated inside dashboards, sidebars, forms, or cards. Metadata uses uppercase mono labels with moderate letter spacing. Body text uses normal letter spacing and comfortable line height for scanability.

## 4. Component Stylings
* **Buttons:** Rectangular command buttons with 8px corners, thin borders, mono labels, and subtle hover lift. Primary actions use Carbon Ink with White Ledger text. Destructive actions use Danger Red outlines and pale red backgrounds.
* **Navigation:** A sticky rectangular command bar with grouped links. Active links are dark and compact; inactive links are pale with visible borders on hover.
* **Cards/Containers:** Low-radius panels, thin rules, and soft shadows. Cards are for repeated entities, modals, and tool panels only; page sections should remain full-width within the shell.
* **Tables:** Ledger-like rows with clear separators, uppercase mono headers, subtle row hover, and horizontal overflow protection.
* **Inputs/Forms:** White fields, strong border, 8px radius, clear focus outline using Signal Teal, and full-width controls in form grids.
* **Status Chips:** Small rectangular tags with 6px corners. Reserve filled dark chips for high-score or active states.

## 5. Layout Principles
Use a constrained shell around 1240px wide. Keep shared pages dense and operational: page header, actions, search/filter region, then data. Forms should use two-column grids on desktop and a single column on mobile. Detail pages should split summary and supporting evidence while keeping all panels aligned to the same spacing scale.

The home/mission experience may remain more immersive, but it must still use the same typography, restrained colors, and sharper controls so it feels connected to the rest of the app.
