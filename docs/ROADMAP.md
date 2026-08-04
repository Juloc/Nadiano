# Alpha, beta and 1.0 roadmap

## 1. Release philosophy

Each milestone must be a complete usable vertical slice. A milestone is not complete because screens exist; the learning loop, data persistence, validation, tests, deployment and documentation must all work together.

Release labels:

- **Prototype:** disposable technical experiment, never deployed as the product.
- **Alpha:** internal household use, limited content, migration and compatibility may still change.
- **Beta:** daily-use candidate with stable core workflows and a feature-complete beginner path.
- **Release candidate:** only defect correction, content correction and release hardening remain.
- **1.0:** supported stable self-hosted release.

## 2. Milestone overview

| Milestone | Primary proof | Planned audience |
|---|---|---|
| A0 | project builds, tests and deploys | developers |
| A1 | browser receives and displays MIDI | developers |
| A2 | one score can be practised and scored | internal learners |
| Alpha | complete foundation learning loop | household |
| B1 | complete beginner course structure | household testers |
| B2 | import, adaptive review and PWA | invited testers |
| Beta | feature-complete 1.0 candidate | invited testers |
| RC1 | upgrade, backup, accessibility and release hardening | release testers |
| 1.0 | stable documented self-hosted product | users |

# 3. Foundation milestones

## A0 — repository and delivery foundation

### Scope

- .NET 10 solution and projects;
- Razor Pages application shell;
- TypeScript build with strict mode;
- SQLite connection and first migration;
- Dockerfile and minimal Compose file;
- health endpoints;
- German and Indonesian interface resources;
- test projects and CI pipeline;
- version information in the UI and diagnostics endpoint.

### Acceptance gate

- clean checkout builds with one documented command;
- tests run without a MIDI device;
- container starts as non-root and reports ready;
- both languages can be selected;
- empty database initializes through committed migrations;
- no high-severity dependency or container scan findings.

## A1 — MIDI and audio foundation

### Scope

- browser capability page;
- secure-context check;
- MIDI permission request initiated by user action;
- input device selection and reconnect handling;
- normalized note-on, note-off and sustain events;
- live keyboard/event display;
- Web Audio metronome with count-in;
- fake MIDI adapter for tests;
- diagnostics export without raw private data.

### Acceptance gate

- supported Chrome/Edge browser connects to a common USB MIDI piano;
- connection loss produces a recoverable message;
- note-on velocity zero is handled correctly;
- sustain events are visible and normalized;
- metronome remains perceptually stable during a two-minute test;
- automated browser tests exercise the same consumer interface through a fake adapter.

## A2 — notation and deterministic scoring

### Scope

- load one bundled MusicXML score;
- render with OpenSheetMusicDisplay;
- generate/load normalized expected events;
- current-note cursor and event highlighting;
- wait, loop and performance modes;
- deterministic pitch and onset matching;
- separate pitch and timing feedback;
- practice session persistence;
- recorded MIDI fixture tests.

### Acceptance gate

- the same fixture always produces the same result;
- chords, repeated notes and extra notes have explicit tested behavior;
- learner can select and repeat measures;
- result identifies measure/beat and next action;
- page reload after a completed attempt does not duplicate the result;
- rendering failure does not corrupt progress.

# 4. Alpha release

## Alpha goal

A learner can create a profile, connect a MIDI piano, complete the first physical/keyboard foundation lessons, practise a short original score and receive useful feedback in German or Indonesian.

## Alpha scope

### Profiles and setup

- create, select, rename and delete local learner profiles;
- language and note-name preference;
- MIDI setup wizard;
- stored preferred input with safe fallback;
- first-run flow and browser support explanation.

### Learning experience

- F0 foundation course;
- selected F1 lessons;
- at least 20 exercises;
- at least 4 listening tasks;
- at least 3 original mini-pieces;
- technique demonstrations with text alternatives;
- one technique cue per pass;
- self-assessment for posture, tension and fingering-related cues.

### Practice engine

- wait, loop, hands-separate and performance modes;
- pitch, onset and basic duration categories;
- count-in and tempo control;
- category-specific result view;
- retry recommended section;
- progress and recent attempts.

### Operations

- Docker image and minimal Compose example;
- persistent volume;
- database migrations;
- export of profile progress as JSON;
- basic backup instructions;
- internal release checklist.

## Alpha exclusions

- MusicXML upload;
- offline practice guarantees;
- generated exercises;
- audio/video recording;
- public accounts;
- advanced pedal evaluation;
- MIDI-to-notation conversion.

## Alpha exit criteria

- two separate learners can use the same deployment without progress mixing;
- complete first-start-to-first-result path works in German and Indonesian;
- all bundled packages pass validation;
- all F0 lessons have reviewed explanations and original media;
- no known data-loss defect;
- no known scoring defect that marks a wrong pitch as correct;
- container can be upgraded once in a rehearsal without losing data;
- five complete manual sessions have been performed on at least two supported browsers and one real digital piano.

# 5. Beta development

## B1 — complete beginner path

### Scope

- complete F0, F1, B1 and B2 course progression;
- generated reading and rhythm cards using seeded reviewed templates;
- ear-training player and answer flows;
- stage checks;
- course map and prerequisite visualization;
- session planner balancing competencies;
- skill evidence and review queue;
- accessibility baseline across all learner pages.

### Acceptance gate

- at least 45 guided lessons and 100 exercises pass content validation;
- generated tasks are reproducible from their seed;
- course cannot be completed by repertoire scores alone;
- delayed review items appear predictably;
- a learner can understand why an item was recommended.

## B2 — import, PWA and adaptive practice

### Scope

- MusicXML and MXL upload;
- archive/XML security limits;
- notation preview and warnings;
- hand/voice mapping;
- section and target-tempo editor;
- fingering display and limited reviewed editing workflow;
- private library;
- PWA installation;
- cached app shell and previously prepared lesson assets;
- resilient active-session result queue;
- adaptive section, hands-separate and tempo recommendations.

### Acceptance gate

- malformed or hostile packages fail safely with actionable errors;
- imported and bundled lessons use the same runtime practice path;
- offline interruption does not duplicate a completed result;
- private imported files are not placed in a shared cache;
- import warnings distinguish unsupported from invalid content;
- adaptive recommendations are derived from explicit tested rules.

## Beta release scope

Everything planned for 1.0 is functionally present except final hardening, final content quantity and release support documentation.

## Beta exit criteria

- full first-start, daily-session, import and restore workflows pass browser tests;
- no open critical/high security findings;
- no known data-loss or cross-profile privacy defect;
- database and content-schema migrations are tested from the alpha version;
- German and Indonesian key parity is complete;
- manual test matrix covers two Chromium-based desktop browsers and supported device classes;
- beta testers can use the app for at least two weeks without manual database repair;
- all 1.0-blocking issues are classified and assigned.

# 6. Release candidate

## RC1 scope

- complete 1.0 content quantities;
- introductory E1 lessons selected for the beginner endpoint;
- final original/public-domain repertoire review;
- backup and restore UI or documented assisted workflow;
- profile data deletion and export;
- accessibility audit and fixes;
- performance profiling on modest hardware;
- dependency update and license report;
- container hardening;
- release notes and upgrade guide;
- telemetry/privacy review;
- browser compatibility documentation;
- disaster-recovery rehearsal.

## RC1 exit criteria

- all 1.0 acceptance criteria pass;
- no release-blocking defects;
- all bundled content has musical, pedagogical, localization and licensing sign-off;
- upgrade from latest alpha/beta test fixtures preserves progress;
- backup restore onto a clean deployment succeeds;
- WCAG-focused automated checks and manual keyboard checks pass for core workflows;
- image is reproducible and tagged with version plus commit SHA;
- rollback instructions are tested.

# 7. Version 1.0

## Functional scope

### Learning

- at least 60 guided lessons through foundation, beginner and introductory elementary material;
- at least 120 technique/rhythm exercises;
- at least 80 reading configurations;
- at least 60 ear-training tasks;
- at least 24 original mini-pieces;
- 12 verified public-domain melodies in Nadiano editions;
- stage checks and final beginner assessment;
- deliberate-practice recommendations and review scheduling.

### Practice

- wait, rhythm, loop, hands separate, tempo ladder, listen/copy, performance and sight-reading modes where content supports them;
- pitch, timing, steadiness, duration, articulation, basic dynamics and pedal observations with honest limitations;
- section-focused next actions;
- profile-specific history and skill evidence.

### Content and library

- versioned package schemas;
- bundled content validation;
- MusicXML/MXL private import;
- private package export;
- safe content storage and attribution records.

### Product and operations

- German and Indonesian;
- PWA installation and controlled caching;
- one-container Docker deployment;
- persistent data volume;
- health checks, structured logs and diagnostics export;
- profile export/delete;
- tested backup, restore and upgrade;
- accessibility baseline;
- published architecture and contributor documentation.

## 1.0 release blockers

Any of the following blocks release:

- wrong notes accepted as correct in a normal tested case;
- progress loss or cross-profile mixing;
- import path traversal, XML entity or archive expansion vulnerability;
- application unusable after MIDI disconnect;
- bundled lesson missing required translation or attribution;
- deployment requires undocumented manual database changes;
- restore procedure not tested;
- unsupported browser shown as supported;
- physical technique automatically marked correct from MIDI-only evidence;
- core workflow inaccessible by normal pointer/keyboard controls.

# 8. Post-1.0 direction

## 1.x

- expanded E1 and intermediate course content;
- improved pedal and dynamics analysis;
- teacher notes on private profiles;
- better printable and large-score layouts;
- more languages and notation naming systems;
- optional local audio recording.

## 2.0 candidates

- assisted MIDI-to-MusicXML workflow;
- teacher/learner account model;
- synchronized multi-device profiles;
- optional camera-assisted self-review;
- richer notation editing;
- licensed content distribution.

These candidates require new architecture, privacy and licensing decisions and are not implied promises.

# 9. Milestone management

Each milestone should be represented by GitHub issues grouped by the work packages in `JUNIOR_IMPLEMENTATION_PLAN.md`.

An issue must include:

- user outcome;
- exact scope and exclusions;
- dependencies;
- implementation steps;
- acceptance criteria;
- required tests;
- documentation and localization changes;
- manual verification steps.

Do not create broad issues such as “implement practice mode” without a complete vertical slice and objective completion conditions.
