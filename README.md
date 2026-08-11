# Nadiano

Nadiano is a browser-based piano learning application for two or more learner profiles. It connects to a digital piano through USB MIDI and teaches reading, rhythm, ear training, technique, repertoire and structured practice.

The interface and course content support German (`de`) and Indonesian (`id`). The localization system is designed for additional languages without duplicating lesson logic or notation files.

## Current stable release

**Nadiano 1.0.4**

Production image:

`ghcr.io/juloc/nadiano:1.0.4`

The production Compose definition in `Juloc/docker` exposes host port `18200` to container port `8080` and persists `/data`.

## Product goal

Nadiano helps a complete beginner build correct foundations and continue into selected early-intermediate material. It does not replace a qualified teacher for posture, tension, movement quality or professional interpretation.

MIDI is used only for objectively measurable feedback such as pitch, onset, duration, velocity and pedal events. Nadiano must not claim that MIDI detected posture, tension or the actual finger used.

## Stable 1.0/1.0.4 baseline

- F0, F1, B1 and B2 path plus selected introductory E1 material;
- guided bilingual lessons and validated reading/rhythm/technique/ear content;
- 24 original Nadiano mini-pieces and 12 public-domain melodies in independently authored Nadiano study editions;
- Web MIDI practice with deterministic scoring;
- Wait, Rhythm, Loop, Hands Separate, Tempo Ladder, Listen/Copy, Performance and Sight Reading where content supports them;
- pitch, timing, steadiness, duration/articulation, basic dynamics and pedal observations;
- separate Sustain CC64, Sostenuto CC66 and Soft/una-corda CC67 diagnostics;
- private MusicXML/MXL import and private Nadiano package export;
- review scheduling and skill evidence;
- PWA installability and resilient completed-result recovery;
- separate learner profiles with export/delete;
- non-root one-container Docker deployment with persistent `/data`;
- release-gated backup, cold restore, upgrade and rollback rehearsals;
- automated accessibility and modest-hardware performance gates;
- five primary learner destinations: **Today | Learn | Songs | Train | Progress**;
- score-dominant focused Practice workspace;
- unified bundled/private Songs library;
- progressive MIDI onboarding.

## Canonical plan

**Read [`docs/MASTER_PLAN.md`](docs/MASTER_PLAN.md) first for any product or implementation work.**

It is the single source of truth for:

- product scope and non-goals;
- learning method and curriculum direction;
- feature decisions;
- UI/UX and visual rules;
- adaptive-practice rules;
- current 1.0.4 baseline;
- 1.1/1.2/1.3/2.0 direction;
- work-package sequence;
- definitions of done.

If another planning document disagrees with `MASTER_PLAN.md`, the master plan wins and the conflicting document must be corrected.

## Documentation

- [Canonical master plan](docs/MASTER_PLAN.md)
- [Current version roadmap](docs/ROADMAP.md)
- [Active unfinished backlog](docs/BACKLOG.md)
- [Current UI/UX status and remaining work](docs/UI_UX_BACKLOG.md)
- [Active junior work packages](docs/JUNIOR_IMPLEMENTATION_PLAN.md)
- [Product concept](docs/PRODUCT_CONCEPT.md)
- [Learning curriculum](docs/LEARNING_CURRICULUM.md)
- [Lesson and content design](docs/CONTENT_MODEL.md)
- [Technical architecture](docs/TECHNICAL_ARCHITECTURE.md)
- [Quality and release requirements](docs/QUALITY_AND_RELEASE.md)
- [First run and browser support](docs/FIRST_RUN_AND_BROWSER_SUPPORT.md)
- [Backup and restore](docs/BACKUP_AND_RESTORE.md)
- [Upgrade to 1.0](docs/UPGRADE_TO_1.0.md)
- [Privacy](docs/PRIVACY.md)
- [1.0 release checklist / current manual gates](docs/RELEASE_1_0_CHECKLIST.md)
- [Research basis and sources](docs/RESEARCH_BASIS.md)
- [Architecture decisions](docs/decisions/README.md)
- [Current handover](HANDOVER.md)

Historical Alpha/Beta checklists and limitation documents are intentionally preserved as versioned release evidence. They are not descriptions of the current stable product.

## Stack

- ASP.NET Core 10 Razor Pages
- TypeScript browser modules
- Web MIDI API
- OpenSheetMusicDisplay for MusicXML rendering
- Web Audio API for metronome and reference playback
- EF Core with SQLite
- PWA shell and IndexedDB for resilient practice sessions
- one Linux Docker container plus one persistent data directory

The architecture is intentionally a modular monolith. Microservices, distributed messaging, CQRS/MediatR and a separate frontend SPA framework remain out of scope unless a demonstrated requirement and architecture decision justify them.

## Repository rules

Read [AGENTS.md](AGENTS.md) before making changes. Documentation and implementation must remain consistent. A feature is incomplete when its behavior, tests, content/schema impact, localization, accessibility or user-facing documentation is missing.