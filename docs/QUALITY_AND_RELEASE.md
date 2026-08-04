# Quality and release requirements

## 1. Definition of done

A feature is complete only when all applicable items are satisfied:

- learner or operator behavior works through the normal UI;
- failure states are handled and localized;
- domain behavior has automated tests;
- browser-only behavior has fake-adapter browser tests;
- persistence changes have a migration and upgrade test;
- German and Indonesian resources are complete;
- accessibility requirements are checked;
- documentation and content schemas are updated;
- privacy and security implications are reviewed;
- manual real-device checks are recorded when MIDI or audio behavior changes.

A hidden developer action, manual database edit or undocumented file copy is not a completed workflow.

## 2. Required CI pipeline

Run these jobs for every pull request:

1. Markdown and formatting validation.
2. `npm ci` and frontend type/lint checks.
3. `dotnet restore` with locked dependencies where supported.
4. `dotnet build` in Release mode.
5. unit and integration tests.
6. browser tests using fake MIDI/audio adapters.
7. bundled content and localization validation.
8. Docker image build.
9. dependency, secret and container scan.
10. optional accessibility smoke test for changed core pages.

The default branch must be protected so required jobs cannot be bypassed accidentally.

## 3. Test layers

### Unit tests

Required for:

- MIDI byte normalization;
- sustain state;
- expected-event generation;
- event matching;
- chord and repeated-note rules;
- timing and duration categories;
- recommendation rules;
- course prerequisites and completion;
- review scheduling;
- content validation.

### Integration tests

Required for:

- SQLite migrations and constraints;
- profile data isolation;
- session idempotency;
- course/content loading;
- import staging and cleanup;
- backup metadata;
- localization handlers;
- application readiness.

### Browser tests

Required for:

- first-run language/profile flow;
- MIDI setup with fake adapter;
- practice start, stop and retry;
- reconnect handling;
- score interaction;
- result submission retry;
- import review;
- offline result queue;
- profile export/delete.

### Manual real-device tests

Required before every MIDI-affecting release:

- permission and connection;
- note-on/note-off across keyboard range;
- velocity variation;
- sustain pedal;
- fast repeated notes;
- chords;
- disconnect/reconnect;
- two-minute metronome/practice run;
- browser reload and session restart.

Record piano model, connection type, operating system and browser version. Device-specific behavior is evidence, not a hard-coded product rule without broader confirmation.

## 4. Scoring correctness gates

The scorer must satisfy these invariants:

- one played note cannot satisfy multiple expected attacks;
- a wrong pitch cannot be marked correct because it is close in time;
- tied notation does not require a repeated attack;
- note-on velocity zero is treated as note-off;
- chord tolerance is explicit and tested;
- timing boundaries are deterministic;
- ignored/unscored categories are not shown as successful;
- self-assessed technique is never converted to objective MIDI success;
- result display can trace each statement to normalized evidence.

Maintain golden fixtures for representative lessons. Any intended result change requires an explicit fixture update and review explanation.

## 5. Content quality gate

Every bundled item requires sign-off for:

- correct notes, rhythm, meter and tempo;
- practical reviewed fingering;
- valid stage and prerequisites;
- concise learning goal and explanation;
- one clear physical cue per relevant pass;
- appropriate objective versus self-assessment categories;
- German review;
- Indonesian review;
- media/text alternative;
- source, license and attribution;
- validator success;
- successful runtime render and practice generation.

Generated templates additionally require property tests for range, duration, measure completeness and reproducibility from seed.

## 6. Localization gate

- every required resource key exists in German and Indonesian;
- no user-facing prose is embedded in domain code;
- no database record depends on a translated enum value;
- layouts tolerate longer strings;
- note-name preference remains independent from interface language;
- validation and error paths are translated, not only happy paths;
- terminology glossary is applied consistently.

Recommended glossary categories:

- pitch/note naming;
- rhythm values;
- hand and finger terms;
- articulation and dynamics;
- practice modes;
- error and recommendation wording.

## 7. Accessibility gate

Core workflows must support:

- keyboard-only operation;
- visible focus;
- semantic names and descriptions;
- no color-only feedback;
- reduced motion;
- scalable text and score;
- text alternatives for technique media;
- visual metronome alternative;
- no audio-only instruction;
- understandable validation and recovery messages.

Automated tools are necessary but do not replace manual keyboard and screen-reader review.

## 8. Security gate

Release is blocked by:

- path traversal or archive extraction outside the import area;
- XML external entity processing;
- unrestricted archive expansion;
- active HTML/script execution from imported content;
- cross-profile private content access;
- secrets committed to the repository or image;
- critical/high known vulnerabilities without documented accepted exception;
- container running as root without a reviewed reason;
- missing HTTPS requirement in production guidance;
- unprotected state-changing endpoints.

Security tests must include malformed XML, malicious archive paths, excessive file counts, oversized fields and unsupported media.

## 9. Privacy gate

1.0 local mode should collect no external analytics by default.

Rules:

- do not log raw MIDI events in normal logs;
- do not upload audio/video without explicit action;
- do not cache private imports in shared browser caches;
- make profile export and deletion available;
- show what diagnostics bundle contains before export;
- document retention for attempt evidence;
- keep profile data separate even without internet authentication;
- require a new decision record before adding cloud synchronization or external telemetry.

## 10. Performance budgets

Initial budgets should be measured and adjusted based on modest target hardware:

- ready home page after warm container start: under 2 seconds on local network;
- cached practice shell interactive: under 2 seconds on supported hardware;
- first simple score render: under 2 seconds after content download;
- live MIDI visual response: no visible network dependency;
- no sustained memory growth across ten consecutive practice attempts;
- history and library pages remain responsive with at least 5,000 attempts and 200 imported items;
- Docker idle memory and image size recorded for every stable release.

Do not optimize by removing correctness evidence or accessibility. Profile before changing architecture.

## 11. Database and migration gate

Every migration must:

- have a descriptive name;
- preserve existing data unless deletion is explicitly approved;
- be tested against a fixture from the previous stable version;
- execute before readiness becomes healthy;
- produce an actionable failure log;
- be compatible with backup/restore documentation;
- avoid manual SQL as a required deployment step.

Before a stable release, rehearse upgrade from the latest supported alpha/beta fixture and restore from backup onto a clean database.

## 12. Backup and restore gate

A supported backup contains:

- SQLite database in a consistent state;
- imported content and media;
- application-managed keys/configuration required to read local data;
- a manifest containing application, database and content versions.

The release process must prove:

1. backup creation;
2. validation of produced files;
3. restore into an empty deployment;
4. successful migration if needed;
5. verification of profiles, progress and imports;
6. documented rollback if the upgrade fails.

An untested volume copy is not a supported restore procedure.

## 13. Browser support policy

The first supported target is current stable Chromium-based desktop browsers with Web MIDI support, primarily Chrome and Edge. Browser capability detection is authoritative at runtime.

Documentation must distinguish:

- supported and tested;
- expected to work but not tested;
- unavailable because Web MIDI is missing or restricted;
- non-MIDI learning features still usable.

Do not browser-sniff to claim support. Test required capabilities and secure context directly.

## 14. Release candidate checklist

- [ ] planned milestone scope complete;
- [ ] all required CI jobs pass;
- [ ] no release blockers open;
- [ ] application and content versions finalized;
- [ ] database upgrade rehearsal passes;
- [ ] backup and restore rehearsal passes;
- [ ] real MIDI matrix passes;
- [ ] German and Indonesian review passes;
- [ ] bundled content sign-off complete;
- [ ] accessibility audit complete;
- [ ] dependency and license report reviewed;
- [ ] container scan reviewed;
- [ ] diagnostics/privacy review complete;
- [ ] release notes and known limitations written;
- [ ] upgrade and rollback instructions tested;
- [ ] immutable image and commit SHA recorded.

## 15. Severity and release policy

### Blocker

Data loss, cross-profile leak, critical security issue, unusable setup/practice path or objectively wrong scoring in common material. No release.

### High

Major learning flow broken, import corruption, migration failure, inaccessible core action or repeated device failure. No stable release; beta exception requires explicit decision.

### Medium

Workaround exists in UI, limited content/rendering defect or non-core accessibility issue. May release only when documented and scheduled.

### Low

Cosmetic or minor wording issue without learning impact. May release with normal prioritization.

## 16. Evidence required for milestone completion

The milestone issue must link to:

- passing CI run;
- container image tag and commit;
- completed manual test matrix;
- migration/restore result where applicable;
- content quantity and validation report;
- localization parity result;
- known limitations;
- signed-off acceptance criteria from `ROADMAP.md`.
