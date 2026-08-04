# ADR-0004: Use Razor Pages with focused TypeScript modules

- Status: Accepted
- Date: 2026-08-04

## Context

Most Nadiano workflows are page and form oriented, while MIDI, audio, score rendering and live practice require browser code. A full client-side application framework would duplicate routing, validation and localization without improving MIDI timing.

## Decision

Use ASP.NET Core Razor Pages for page composition, navigation, forms and server workflows. Use strict TypeScript ES modules behind narrow interfaces for MIDI, audio, notation, IndexedDB and the practice workspace.

Do not introduce React, Angular, Vue, Blazor WebAssembly or a generic client state framework for 1.0.

## Consequences

- less client infrastructure and smaller dependency surface;
- normal server-rendered accessibility and localization patterns;
- complex live practice state still needs disciplined TypeScript modules;
- small JSON endpoints are added only where browser modules need them;
- UI components should remain reusable partials/view components rather than copied markup.

## Alternatives considered

- React/SPA: rejected because current workflows do not justify duplicate routing and API layers.
- Blazor WebAssembly: rejected because browser API integration and payload complexity add no current advantage.
- JavaScript without TypeScript: rejected because event and scoring contracts need strict types.

## Reconsideration triggers

- validated interaction requirements make server-rendered page transitions materially obstructive;
- client state becomes too complex despite module boundaries;
- a native/shared UI requirement changes the product architecture.
