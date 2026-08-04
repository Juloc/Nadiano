# ADR-0001: Use a modular monolith

- Status: Accepted
- Date: 2026-08-04

## Context

Nadiano initially serves a small household deployment but includes several domains: profiles, course content, practice, scoring, progress and imports. These need clear boundaries without the operational and cognitive cost of distributed services.

## Decision

Build one ASP.NET Core application organized into feature modules. Keep framework-independent learning, scoring and progression logic in `Nadiano.Core`. Keep hosting, persistence and UI in `Nadiano.Web`.

Do not add microservices, a message bus, distributed transactions or a service-per-feature deployment for 1.0.

## Consequences

- one build, deployment and data backup path;
- direct in-process calls and transactions;
- easier debugging and onboarding;
- module boundaries must be enforced through code review and dependencies rather than network APIs;
- future extraction remains possible only after a measured independent scaling or security need.

## Alternatives considered

- Microservices: rejected because no independent scaling, team or deployment requirement exists.
- Single unstructured web project: rejected because scoring and content rules need testable framework-independent boundaries.
- Full clean-architecture project per layer: rejected because additional projects and mapping would not provide a current benefit.

## Reconsideration triggers

- separate teams need independent deployment ownership;
- one module requires materially different scaling or isolation;
- an external integration needs a stable independently deployed service;
- measured build/runtime constraints cannot be solved inside the monolith.
