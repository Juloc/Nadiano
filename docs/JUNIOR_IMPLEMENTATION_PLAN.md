# Junior implementation plan

## 1. How to use this plan

Implement work packages in order unless a package explicitly allows parallel work. Each package must produce a small complete result and must satisfy its acceptance criteria before the next dependent package starts.

For every package:

1. Read the referenced product and architecture sections.
2. Create or update a GitHub issue with the package scope.
3. Add tests before or with production code.
4. Implement only the stated scope.
5. Run format, build, tests and content validation.
6. Verify German and Indonesian behavior.
7. Update documentation when behavior differs from the plan.
8. Open a reviewable pull request with screenshots or recordings for UI changes.

Do not combine several unfinished packages into one long-lived branch.

# Phase A0 — repository foundation

## WP-001 Create the .NET solution

### Goal

Create a minimal buildable solution with clear boundaries.

### Steps

1. Create `Nadiano.sln`.
2. Create `src/Nadiano.Core` as a .NET 10 class library.
3. Create `src/Nadiano.Web` as an ASP.NET Core 10 Razor Pages application.
4. Create the four test projects described in the architecture document.
5. Enable nullable reference types and implicit usings.
6. Treat compiler warnings as errors in CI, not necessarily every local debug build.
7. Add central package version management.
8. Add `.editorconfig`, `.gitignore` and deterministic build settings.
9. Reference `Nadiano.Core` from the web and relevant test projects.
10. Add a simple domain test proving test discovery works.

### Acceptance criteria

- `dotnet build` succeeds from repository root.
- `dotnet test` finds and runs all test projects.
- `Nadiano.Core` has no ASP.NET Core or EF Core dependency.
- no generated IDE files are committed.

## WP-002 Add frontend build tooling

### Goal

Compile strict TypeScript and CSS without introducing a SPA framework.

### Steps

1. Add `package.json` and lock file under `src/Nadiano.Web` or repository root.
2. Configure TypeScript strict mode and ES modules.
3. Add build scripts for development and production.
4. Output versioned assets into `wwwroot/dist`.
5. Add one small entry module and verify loading from Razor layout.
6. Add linting with a small documented rule set.
7. Exclude generated frontend assets from manual editing.
8. Integrate frontend build into `dotnet publish` or the Docker build.

### Acceptance criteria

- `npm ci && npm run build` is deterministic.
- TypeScript compilation fails on type errors.
- production pages do not load source files or development servers.
- no frontend framework is installed.

## WP-003 Add application shell and localization

### Goal

Create a simple accessible shell supporting German and Indonesian.

### Steps

1. Add navigation for Home, Learn, Practice, Library, Progress and Settings.
2. Add culture selection stored in a cookie or selected profile later.
3. Create paired German and Indonesian resources.
4. Add language parity test that detects missing keys.
5. Add skip link, focus styles and semantic landmarks.
6. Show application version in an About or diagnostics view.
7. Add generic localized validation and error page.

### Acceptance criteria

- every initial page can render in both languages.
- changing language does not require editing the URL manually.
- all controls have accessible labels.
- localization parity test passes.

## WP-004 Add SQLite persistence

### Goal

Create reliable local persistence using committed migrations.

### Steps

1. Add EF Core SQLite to `Nadiano.Web`.
2. Create `NadianoDbContext` under Infrastructure/Persistence.
3. Add initial entities only for learner profile and application schema state.
4. Add explicit entity configurations.
5. Create and commit the first migration.
6. Apply migrations during controlled startup before readiness is reported.
7. Configure database path under `/data` with local development fallback.
8. Add integration tests using temporary SQLite files, not the in-memory provider.

### Acceptance criteria

- a fresh database is created by migration.
- restarting preserves data.
- a failed migration prevents readiness and logs an actionable error.
- integration tests exercise real SQLite constraints.

## WP-005 Add Docker and CI foundation

### Goal

Build and validate one non-root container.

### Steps

1. Add multi-stage Dockerfile for frontend build, .NET publish and runtime.
2. Run as a dedicated non-root user.
3. Expose port 8080 and persist `/data`.
4. Add `/health/live` and `/health/ready`.
5. Add minimal Compose example.
6. Add CI jobs for format check, frontend build, .NET build, tests and container build.
7. Add dependency and container vulnerability scanning.
8. Publish only after tests pass.

### Acceptance criteria

- container starts from a clean checkout.
- runtime image does not contain SDK or npm cache.
- write access is limited to required paths.
- health checks reflect database readiness.
- CI fails for test or scan policy failures.

# Phase A1 — MIDI and audio foundation

## WP-006 Define browser capability contracts

### Goal

Isolate browser APIs behind typed interfaces.

### Steps

1. Define `MidiAccessAdapter`, `MidiInputDevice`, `AudioClock` and `LocalSessionStore` interfaces.
2. Define normalized MIDI event types.
3. Add a production Web MIDI implementation.
4. Add fake implementations for browser tests.
5. Expose capability results: secure context, MIDI availability, audio availability and IndexedDB availability.
6. Do not request permissions during page load.

### Acceptance criteria

- consumer modules do not access `navigator.requestMIDIAccess` directly.
- fake adapter can emit events in deterministic order.
- unsupported capability produces a typed result, not a thrown null-reference error.

## WP-007 Build MIDI setup page

### Goal

Allow a learner to grant permission, select a piano and understand failures.

### Steps

1. Add user-initiated “Connect MIDI” action.
2. List available input devices after permission.
3. Show selected device and connection state.
4. Detect device connect/disconnect state changes.
5. Display localized guidance for insecure origin, denied permission and unsupported browser.
6. Store preferred device identifier plus name as a hint, not as an absolute guarantee.
7. Add “forget device preference”.

### Acceptance criteria

- no permission prompt appears without a learner action.
- disconnect and reconnect do not require page reload in normal cases.
- selecting one device prevents unrelated MIDI input from being used.
- no raw device identifier is exposed in normal logs.

## WP-008 Normalize MIDI events

### Goal

Create a tested normalized event stream.

### Steps

1. Parse note-on, note-off and control-change messages.
2. Convert note-on velocity zero to note-off.
3. Preserve channel, pitch, velocity and timestamps.
4. Track sustain pedal controller 64.
5. Assign monotonic sequence numbers.
6. Ignore unsupported messages safely while offering diagnostics counters.
7. Add unit tests for malformed and boundary messages.

### Acceptance criteria

- event fixtures cover all required message forms.
- no event is rounded or converted to musical beats in this layer.
- malformed input cannot crash the session.

## WP-009 Add live MIDI diagnostics

### Goal

Make connection issues testable without exposing complex developer tools.

### Steps

1. Display a simple 88-key keyboard view.
2. Highlight active notes.
3. Show velocity and sustain state.
4. Add optional compact event list limited to recent events.
5. Add diagnostics export with app version, capability result and sanitized device information.
6. Ensure the page works with the fake adapter in browser tests.

### Acceptance criteria

- active notes clear correctly after note-off and disconnect.
- diagnostics do not contain profile prose or full raw practice history.
- keyboard view uses text/non-color status alternatives.

## WP-010 Implement Web Audio metronome

### Goal

Provide a stable count-in and practice clock.

### Steps

1. Create/resume AudioContext only after user interaction.
2. Use look-ahead scheduling for clicks.
3. Support BPM, meter and accented first beat.
4. Provide audible and visual beat output.
5. Return a monotonic start time for practice sessions.
6. Handle tab visibility changes and suspend/resume explicitly.
7. Add clock tests around schedule calculation.

### Acceptance criteria

- changing tempo stops old scheduling cleanly.
- two-minute manual run has no accumulating visible drift.
- visual metronome respects reduced-motion preference.

# Phase A2 — content, notation and scoring slice

## WP-011 Implement content schemas and validator

### Goal

Load one bundled lesson through the future production content path.

### Steps

1. Add versioned manifest models matching `CONTENT_MODEL.md`.
2. Add schema/reference/localization validators.
3. Add skill catalogue loading.
4. Add valid and invalid fixture packages.
5. Create CLI content validator in `tools/Nadiano.ContentValidator`.
6. Run validation during CI and application startup for bundled content.
7. Return structured validation errors for user imports later.

### Acceptance criteria

- bundled invalid content fails CI.
- validation reports file, field and reason.
- the web page never reads arbitrary JSON properties dynamically.

## WP-012 Integrate MusicXML rendering

### Goal

Render a bundled score reliably and map score positions to expected events.

### Steps

1. Install a pinned OpenSheetMusicDisplay version.
2. Serve dependencies locally.
3. Create a notation adapter module.
4. Load score from an application endpoint or controlled static path.
5. Handle render, resize, zoom and error state.
6. Add cursor and measure-selection support.
7. Establish stable mapping between expected event IDs and rendered notes.
8. Add browser fixture with a small original score.

### Acceptance criteria

- no CDN is required.
- score remains usable on common tablet and desktop widths.
- unsupported score produces a localized error rather than an empty page.
- loop range can be selected by measure.

## WP-013 Build expected-event generation

### Goal

Produce normalized scoring input from reviewed lesson content.

### Steps

1. Start with a limited supported MusicXML subset.
2. Read parts, measures, voices, pitch, onset, duration, tempo and fingering.
3. Generate stable event identifiers.
4. Support chords and ties explicitly.
5. Produce versioned `expected-events.json` during content build.
6. Compare generated output with committed fixtures.
7. Fail content validation when unsupported notation affects scoring.

### Acceptance criteria

- generation is deterministic.
- ties do not create false repeated attacks.
- chords are represented as one expected onset group where appropriate.
- unsupported scoring constructs are reported, not guessed silently.

## WP-014 Implement core event matcher

### Goal

Match played and expected events deterministically.

### Steps

1. Define `ScoringPolicy` with timing windows and mode behavior.
2. Match single notes by pitch and nearest eligible onset.
3. Match chord groups with configurable roll tolerance.
4. Identify omissions, additions, early and late attacks.
5. Preserve raw deviations for later formatting.
6. Add fixture tests for repeated notes, chords and overlapping sustain.
7. Keep matcher independent from localization and UI.

### Acceptance criteria

- same input produces byte-equivalent normalized result where serialization order is defined.
- wrong pitch cannot be matched merely because timing is close.
- one played event cannot satisfy two expected attacks.
- tests document ambiguous boundary behavior.

## WP-015 Add scoring categories and feedback facts

### Goal

Return useful evidence instead of one score.

### Steps

1. Add pitch correctness and error locations.
2. Add onset deviation and timing-band classification.
3. Add basic duration ratio.
4. Add steadiness calculation for repeated pulse patterns.
5. Add minimal velocity range facts without claiming acoustic tone quality.
6. Add pedal event observations only when content declares pedal expectations.
7. Produce a recommended next-action code from explicit rules.
8. Localize the display separately.

### Acceptance criteria

- every displayed claim can be traced to a result fact.
- categories can be disabled by lesson.
- no physical technique skill is automatically passed.

## WP-016 Build first practice workspace

### Goal

Complete one end-to-end practice attempt.

### Steps

1. Show lesson goal, notation, device state, tempo and mode.
2. Add count-in and start/stop controls.
3. Implement wait mode.
4. Implement measure loop mode.
5. Implement uninterrupted performance mode.
6. Highlight current/correct/incorrect/missed events.
7. Show category result and one recommended next action.
8. Add retry for the recommended section.
9. Preserve current section and tempo after recoverable failure.

### Acceptance criteria

- learner can complete a bundled lesson using real or fake MIDI.
- stopping a session releases listeners and scheduled audio.
- repeated attempts do not leak event handlers.
- result is understandable without opening diagnostics.

## WP-017 Persist practice sessions idempotently

### Goal

Store attempts without duplicates or partial corruption.

### Steps

1. Add session and attempt entities.
2. Generate client session/attempt IDs.
3. Add create and complete endpoints.
4. Make completion idempotent.
5. Store normalized summary and content version.
6. Keep raw events only when explicitly required by a short retention setting.
7. Add integration tests for retry and duplicate submission.

### Acceptance criteria

- resubmitting completion returns the existing result.
- content version used by the attempt is preserved.
- one profile cannot read another profile's attempts.

# Phase Alpha — complete internal learning loop

## WP-018 Implement learner profiles

### Goal

Support independent household learners.

### Steps

1. Add create/select/rename/delete flows.
2. Add current-profile cookie or server session identifier.
3. Store language, note-name system, session length and MIDI preference.
4. Require explicit confirmation before deleting progress.
5. Add profile export of structured data.
6. Add authorization filters based on current profile even without internet accounts.

### Acceptance criteria

- progress and imports never cross profile boundaries.
- no practice page opens without a selected profile.
- profile deletion removes or schedules deletion of owned private data.

## WP-019 Implement course progression

### Goal

Expose prerequisites and completion for the first course.

### Steps

1. Load course manifest and ordered stages.
2. Calculate available/locked/completed state.
3. Store enrollment and lesson progress.
4. Implement completion rule evaluation from attempts plus required self-check.
5. Add course map and recommended next lesson.
6. Explain locked prerequisites.

### Acceptance criteria

- manual URL navigation cannot complete locked content accidentally.
- completion is recalculated deterministically.
- course progress is profile-specific.

## WP-020 Build technique lesson presentation

### Goal

Teach non-MIDI physical concepts honestly and clearly.

### Steps

1. Build reusable lesson layout for goal, why, demonstration, common mistake and steps.
2. Support top/side media views.
3. Add text alternatives and reduced-motion fallback.
4. Add dry-task completion prompt.
5. Add one active technique cue for the practice pass.
6. Add up to three self-check questions after playing.
7. Store self-check as learner evidence, not objective truth.

### Acceptance criteria

- the page never says the app detected posture or finger use from MIDI.
- media can be completed without audio.
- one cue is visually primary during practice.

## WP-021 Produce and validate alpha content

### Goal

Create the minimum reviewed content set.

### Steps

1. Write original German source copy for seven F0 lessons and selected F1 lessons.
2. Translate and review Indonesian copy.
3. Compose original short exercises and three mini-pieces.
4. Engrave MusicXML with reviewed fingering.
5. Produce short original demonstrations and reference audio.
6. Add attribution/license records.
7. Validate every package.
8. Perform musical and language review outside the implementation author where possible.

### Acceptance criteria

- quantity targets from the curriculum are met.
- all content loads through the package system.
- no copied textbook prose or modern score edition is committed.
- every item has German and Indonesian content.

## WP-022 Add recent progress and session summary

### Goal

Show useful progress without gamification replacing learning.

### Steps

1. Add recent practice list.
2. Show competency distribution and current recommendations.
3. Show category trends only with enough attempts.
4. Add lesson completion and review due indicators.
5. Avoid punitive streak loss or global competitive points.
6. Add clear data explanation.

### Acceptance criteria

- learner can identify what to practise next.
- charts have text equivalents.
- small data sets are not presented as reliable trends.

## WP-023 Alpha release hardening

### Goal

Prepare the first household deployment.

### Steps

1. Add first-run setup and browser support documentation.
2. Add backup instructions for `/data`.
3. Add database/content version diagnostics.
4. Run real-device test matrix.
5. Fix all alpha blockers from `ROADMAP.md`.
6. Tag and publish immutable alpha container image.
7. Add release notes and known limitations.

### Acceptance criteria

All Alpha exit criteria in `ROADMAP.md` are satisfied and evidenced in the release issue.

# Phase B1 — complete beginner learning system

## WP-024 Add rhythm mode

- allow lesson-defined pitch simplification;
- assess onset, duration and pulse;
- support clap/tap input through pointer/keyboard when no MIDI pitch is needed;
- provide subdivision/counting prompts;
- test syncopation boundaries only within the supported beginner subset.

## WP-025 Add ear-training engine

- schedule local reference tones/phrases through Web Audio or licensed local samples;
- support direction, same/different, short imitation and rhythm echo tasks;
- limit replays according to lesson design;
- store answer and performed-response evidence separately;
- ensure visual UI does not reveal answers prematurely.

## WP-026 Add generated reading and rhythm cards

- implement reviewed template schemas;
- use seeded deterministic generation;
- enforce range, interval, rhythm and clef constraints;
- generate MusicXML/expected events or a supported internal representation;
- store seed with attempt;
- add property-based tests for range and measure validity.

## WP-027 Add review scheduling

- start with explicit rule-based intervals;
- create review queue from skill evidence;
- increase or reduce interval based on category result;
- prevent one high repertoire score from clearing unrelated skills;
- explain why an item is due;
- test date handling across cultures and time zones.

## WP-028 Add adaptive practice rules

- detect repeated error category and location;
- recommend hands separate, smaller section, slower tempo, rhythm-only or listen/copy;
- implement tempo ladder with configurable increments;
- keep every rule explicit and testable;
- show one primary recommendation and optional alternatives;
- never diagnose injury or tension automatically.

## WP-029 Complete B1/B2 course content

- create remaining guided lessons, exercises, reading templates, ear tasks and pieces;
- review fingering and keyboard ranges;
- verify progression and prerequisites;
- add stage checks;
- perform bilingual and licensing review;
- meet B1 content quantities.

## WP-030 Accessibility baseline

- automated accessibility test for core pages;
- manual keyboard-only pass;
- focus management in practice dialogs;
- non-color score feedback;
- text equivalents for notation and progress summaries;
- reduced motion and scalable score;
- audible and visual metronome alternatives.

# Phase B2 — import and resilient PWA

## WP-031 Implement secure upload staging

- define file count/size limits;
- store uploads outside served paths;
- generate safe internal names;
- disable XML external entities;
- inspect MXL archive paths and expansion limits;
- delete failed/abandoned staging data on schedule;
- add hostile fixture tests.

## WP-032 Implement MusicXML/MXL import review

- parse supported notation;
- show score preview and structured warnings;
- select parts, hands and voices;
- review tempo and event extraction;
- define sections and practice modes;
- enter localized title/instructions;
- validate and publish private package.

## WP-033 Add fingering and section metadata editor

- display existing MusicXML fingering;
- allow reviewed overlay changes without building a full score editor;
- support alternate fingerings only through explicit variants;
- define measure ranges and target tempos;
- warn when edits cannot round-trip cleanly;
- preserve original upload.

## WP-034 Add private library lifecycle

- list, search, open, export and delete private packages;
- show source, version, validation state and owner;
- prevent bundled ID overwrite;
- delete related stored files safely;
- do not expose one profile's library to another.

## WP-035 Add PWA shell and offline-safe session recovery

- add manifest and install metadata;
- cache versioned static assets;
- cache only explicitly prepared public/bundled lesson assets;
- store active-session state in IndexedDB;
- queue idempotent completed result;
- show online/offline state;
- add clear-offline-data action;
- test application upgrade cache invalidation.

## WP-036 Beta hardening

- migrate from alpha fixtures;
- run two-week internal usage test;
- profile performance and memory during long practice sessions;
- validate import failure behavior;
- complete privacy review;
- publish beta upgrade and rollback documentation;
- satisfy all Beta exit criteria.

# Phase RC/1.0 — complete stable product

## WP-037 Complete 1.0 content catalogue

- meet all curriculum quantity targets;
- add introductory E1 material selected for the final beginner outcome;
- create Nadiano engravings of verified public-domain melodies;
- review every license and attribution;
- run musical, pedagogical and bilingual sign-off.

## WP-038 Complete remaining practice modes

- hands-separate voice filtering;
- listen-and-copy;
- tempo ladder;
- sight-reading attempt controls;
- basic articulation/dynamics/pedal observations;
- ensure each mode is enabled only for compatible content.

## WP-039 Add backup, restore, export and deletion

- document exact `/data` contents;
- provide database-consistent backup command or application-assisted workflow;
- validate backup before declaring success;
- restore onto a clean deployment;
- export a profile in versioned format;
- delete profile and private files;
- add disaster-recovery test.

## WP-040 Add release diagnostics and support bundle

- show application, database, content and dependency versions;
- include browser capability snapshot;
- include sanitized recent error IDs;
- exclude raw MIDI, lesson prose and private imports by default;
- make bundle reviewable before download.

## WP-041 Security and dependency hardening

- restrictive CSP and security headers;
- verify non-root and writable directories;
- update dependencies to reviewed supported versions;
- generate dependency/license report;
- resolve critical/high findings;
- test import limits and malformed requests;
- verify no CDN/runtime external dependency.

## WP-042 Performance hardening

- set measurable page and practice startup budgets;
- test on modest tablet/laptop hardware;
- virtualize or limit long history lists;
- avoid re-rendering full score for every MIDI event;
- verify no listener or audio-node leak after repeated sessions;
- optimize only after profiling.

## WP-043 Final accessibility and localization audit

- complete keyboard and screen-reader checks on core workflows;
- test German and Indonesian layout expansion;
- verify terminology consistency;
- review audio/text alternatives;
- verify reduced motion and contrast;
- correct all release-blocking findings.

## WP-044 Release pipeline and versioning

- semantic application version;
- immutable commit-SHA container tag;
- release notes generated from reviewed changes;
- migration and compatibility check before publish;
- signed or provenance-enabled build where available;
- stable tag only after release gate passes;
- retain rollback image.

## WP-045 1.0 release rehearsal

1. Start from a supported alpha/beta backup fixture.
2. Deploy the release candidate.
3. Run migrations and verify readiness.
4. Connect a real MIDI piano.
5. Complete setup, lesson, generated exercise, imported lesson and result review.
6. Stop and restart the container.
7. Export one profile.
8. Create a backup.
9. Restore onto a clean deployment.
10. Verify profile, progress, imports and content versions.
11. Execute rollback procedure.
12. Record evidence in the release issue.

### Acceptance criteria

All 1.0 release blockers are closed, RC exit criteria pass and the release rehearsal requires no undocumented repair.

# 2. Pull request checklist

Every pull request should answer:

- What learner or operator outcome changes?
- Which work package and acceptance criterion does it implement?
- What is intentionally excluded?
- Which tests prove the behavior?
- Which German and Indonesian resources changed?
- Does content schema, database schema or stored data change?
- Does the change affect privacy, browser compatibility or deployment?
- What manual verification was completed?

# 3. Junior escalation rules

Stop and request architecture review when:

- a new framework or service appears necessary;
- browser and server scoring disagree;
- a MusicXML construct cannot be represented without guessing;
- a migration could lose progress or imported content;
- physical technique would be inferred from MIDI;
- copyrighted content status is uncertain;
- a workaround duplicates an existing module;
- security requires accepting active HTML, scripts or external entities;
- test behavior differs between fake and real MIDI without a known device cause.

Do not hide these problems behind feature flags, catch-all exceptions or undocumented manual steps.
