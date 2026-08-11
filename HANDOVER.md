# Nadiano handover — current stable baseline

Current stable release: **1.0.4**

Canonical plan: `docs/MASTER_PLAN.md`

Active work packages: `docs/JUNIOR_IMPLEMENTATION_PLAN.md`

Active unfinished work: `docs/BACKLOG.md`

Version sequence: `docs/ROADMAP.md`

## Current product state

Nadiano is no longer in Alpha/Beta implementation. The stable 1.0 learning, MIDI, content, import, PWA, backup/restore and release foundations are complete.

1.0.4 additionally delivered the structural learner UI redesign.

### Stable learner navigation

Normal learner navigation:

- Today;
- Learn;
- Songs;
- Train;
- Progress.

Profile, settings, language, MIDI setup and diagnostics are secondary.

### Today

Today currently uses real progress data to show due reviews, the recommended next lesson and course state.

The next major feature is the full 10/20/30-minute session composer defined in WP-047.

### Practice

Current Practice is score-dominant and keeps essential mode/tempo/hand/loop/zoom/start-stop controls accessible without the old long vertical form.

Results prioritize problem location and a concrete next action.

Next major practice improvement: adaptive micro-practice/mastery flow (WP-048).

### Songs

Bundled Nadiano repertoire and private MusicXML/MXL imports share one Songs surface and one Practice path.

Current filters include text/source/status. Favorites and richer skill/difficulty/recommendation filters remain planned.

### MIDI

Real-device evidence confirms the complete keyboard is recognized.

The setup UI separately detects:

- Sustain CC64;
- Sostenuto CC66;
- Soft/una-corda CC67.

The setup flow is progressive: capability → permission → device → key/pedal test → completion.

### Content

Stable content includes the complete F0/F1/B1/B2 path plus selected introductory E1 material, validated technique/rhythm/reading/ear tasks, original mini-pieces and public-domain Nadiano study editions.

Do not treat historical Alpha handover instructions as pending content work.

### Operations

Stable release pipeline verifies:

- frontend build/lint/tests/audit;
- .NET format/build/tests;
- content validation;
- browser/accessibility path;
- Docker image;
- Trivy;
- 1 CPU / 512 MiB performance profile;
- upgrade;
- cold restore;
- rollback;
- dependency/license reports.

Production Compose in `Juloc/docker` uses `ghcr.io/juloc/nadiano:1.0.4`, host port `18200`, container port `8080`, persistent `/data`.

## Manual gates still open

Do not claim these as automated passes:

- German real-permission Chrome first-run → result;
- Indonesian real-permission Edge first-run → result;
- production HTTPS PWA install + MIDI reconnect;
- manual keyboard-only review;
- human musical/pedagogical/localization/licensing sign-off;
- sustained household/invited-user use without manual DB repair.

See `docs/RELEASE_1_0_CHECKLIST.md`.

## Next work order

1. WP-047 — Today session composer.
2. WP-048 — adaptive micro-practice.
3. WP-049 — progressive assistance fading.
4. WP-050 — skill progress + adaptive sight reading.
5. WP-051 — chord/accompaniment vertical slice.
6. WP-052 — imported-piece practice intelligence.
7. WP-053 — repertoire/technique refinement.
8. WP-054 — 1.1 release hardening.

## Architecture rules to preserve

- modular monolith;
- ASP.NET Core Razor Pages;
- TypeScript only for browser-specific MIDI/audio/notation/practice behavior;
- `Nadiano.Core` remains framework-independent;
- EF Core + SQLite;
- one Docker container + `/data`;
- browser owns Web MIDI; server never directly accesses USB;
- deterministic scoring/adaptation for fixed inputs;
- no microservices, CQRS, MediatR, event bus, generic repository framework or SPA framework without an approved architecture decision;
- no mandatory cloud service for core learning.

## Learning rules to preserve

- standard notation remains primary;
- never claim MIDI detects posture/tension/actual finger;
- objective evidence and self-assessment stay separate;
- feedback gives category + location + next action;
- bundled/imported pieces use the same runtime;
- aids should eventually fade with mastery rather than become permanent;
- no global leaderboards/loot boxes/aggressive streak punishment;
- generic AI chat is not the scoring/progression authority.

Before starting new work, read `docs/MASTER_PLAN.md`. If another planning document conflicts with it, correct the conflicting document rather than guessing which plan to follow.