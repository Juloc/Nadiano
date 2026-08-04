# Technical architecture

## 1. Architecture goal

Nadiano should be easy to run in one Docker container, easy to understand by a junior developer and capable of evaluating MIDI locally in the browser with predictable latency.

The architecture is a modular monolith:

```text
Digital piano
    │ USB MIDI
    ▼
Learner browser
  - Web MIDI adapter
  - local practice clock
  - event normalization
  - live scoring session
  - notation interaction
    │ HTTPS: results, content, settings
    ▼
Nadiano ASP.NET Core container
  - Razor Pages UI
  - course and content services
  - profile and progress services
  - import and validation services
  - SQLite persistence
    │
    ▼
Persistent volume
```

The server does not access the learner's USB piano. USB access is granted by the browser on the learner's device.

## 2. Technology choices

### Server

- .NET 10 LTS;
- ASP.NET Core Razor Pages;
- EF Core with SQLite;
- built-in ASP.NET Core localization, authentication primitives where later required, health checks and logging.

### Browser

- TypeScript compiled into small ES modules;
- Web MIDI API for supported browsers;
- Web Audio API for clocked metronome and tones;
- OpenSheetMusicDisplay for MusicXML rendering;
- IndexedDB for an active-session recovery buffer and cached content metadata;
- Service Worker only for installability and controlled static/content caching.

### Deployment

- multi-stage Docker build;
- non-root runtime user;
- one persistent `/data` volume;
- HTTP inside the container;
- HTTPS supplied by the reverse proxy because Web MIDI requires a secure context;
- health endpoint for orchestration.

## 3. Why no SPA framework

The product mainly contains pages, dialogs, lesson content and one complex practice workspace. Razor Pages handles navigation, forms, localization and server data directly. TypeScript modules handle browser-only MIDI, audio and notation interaction.

A SPA framework is not justified for the first release because it would add:

- duplicate client routing and state infrastructure;
- a separate API for simple server workflows;
- larger dependency and build surfaces;
- more code for localization and validation;
- no direct benefit to MIDI timing, which still needs dedicated browser modules.

This decision can be revisited only after measured UI requirements cannot be handled cleanly.

## 4. Solution structure

```text
src/
  Nadiano.Web/
    Features/
      Home/
      Profiles/
      Setup/
      Courses/
      Practice/
      Library/
      Imports/
      Progress/
      Settings/
    Infrastructure/
      Persistence/
      FileStorage/
      Localization/
    Pages/
    wwwroot/
      js/
        midi/
        audio/
        notation/
        practice/
      css/
  Nadiano.Core/
    Content/
    Courses/
    Practice/
    Scoring/
    Progress/
    Profiles/
    Common/
content/
  courses/
  skills/
  schemas/
tests/
  Nadiano.Core.Tests/
  Nadiano.Web.IntegrationTests/
  Nadiano.BrowserTests/
  Nadiano.ContentTests/
tools/
  Nadiano.ContentValidator/
docs/
```

Do not add separate Application, Domain and Infrastructure projects merely to follow a diagram. `Nadiano.Core` contains framework-independent behavior. `Nadiano.Web` contains hosting, persistence adapters and UI. Split a project only when compilation boundaries provide a concrete benefit.

## 5. Module boundaries

### Profiles

Owns learner preferences and identity within the household. No internet account is required for 1.0.

### Content

Loads bundled and private packages, validates manifests, resolves translations and provides normalized lesson definitions.

### Courses

Calculates prerequisites, availability, stage completion and recommended path.

### Practice

Creates a practice session from lesson content and mode configuration. Owns count-in, loops, tempo ladders and session state.

### Scoring

Matches expected events to played events and produces category-specific observations. It does not decide course progression directly.

### Progress

Stores attempts, aggregates skill evidence and schedules review work.

### Imports

Accepts MusicXML/MXL, applies security limits, presents review state and publishes validated private packages.

### Browser adapters

Own Web MIDI, Web Audio, notation rendering and IndexedDB. They communicate with server code through narrow typed DTOs.

## 6. MIDI pipeline

```text
MIDIInput message
  -> parse status/channel/data bytes
  -> normalize note-on, note-off, control change
  -> timestamp against performance clock
  -> sustain-state processing
  -> active-note state
  -> practice matcher
  -> live visual feedback
  -> completed attempt summary
```

### Normalized event

```ts
export type PlayedMidiEvent = {
  sequence: number;
  kind: "noteOn" | "noteOff" | "controlChange";
  receivedAtMs: number;
  deviceTimestampMs?: number;
  channel: number;
  note?: number;
  velocity?: number;
  controller?: number;
  value?: number;
};
```

Rules:

- treat note-on with velocity zero as note-off;
- keep raw and normalized timestamps for diagnostics;
- use one monotonic browser clock for the practice session;
- do not round events before matching;
- normalize sustain pedal state explicitly;
- ignore unsupported system messages unless a feature declares support;
- record device metadata without assuming the product name is stable.

## 7. Practice clock and latency

Live evaluation runs in the browser to avoid a network round trip. The metronome uses Web Audio scheduling ahead of playback rather than repeated `setInterval` clicks.

Before a scored session:

1. create/resume the audio context after user interaction;
2. schedule count-in beats;
3. establish a practice start timestamp;
4. compare normalized MIDI events against expected beat positions;
5. apply configured tolerance based on stage and mode.

The app may offer a calibration workflow, but must not hide wide timing errors behind unlimited tolerance.

## 8. Scoring design

The core scorer is deterministic and pure where practical:

```text
Expected events + played events + scoring policy
    -> matched pairs
    -> omissions and additions
    -> category observations
    -> section summary
```

Separate policies cover:

- wait mode;
- rhythm mode;
- normal guided practice;
- performance mode;
- chord matching;
- pedal observations.

The scorer returns facts such as `onsetLateByMs` and `unexpectedPitch`. A feedback formatter converts facts into age-neutral learner language.

Do not store only a percentage. Store enough normalized evidence to explain the result, while avoiding raw MIDI retention longer than needed by the configured privacy setting.

## 9. Notation integration

OpenSheetMusicDisplay renders MusicXML to SVG. Nadiano owns:

- score loading and error handling;
- mapping MusicXML/event IDs to rendered notes;
- current-position cursor;
- highlighting expected, correct, incorrect and missed events;
- responsive layout and zoom;
- finger-number visibility;
- measure selection for loops.

Do not build a full notation editor for the beta. The import editor changes only Nadiano metadata, hand mapping, fingering overlays where supported, section boundaries and practice settings.

## 10. Server communication

Prefer normal Razor Page handlers and small JSON endpoints. Example endpoints:

```text
GET  /practice/{lessonId}
POST /api/practice/sessions
POST /api/practice/sessions/{id}/complete
GET  /api/content/{lessonId}/expected-events
POST /api/imports/musicxml
POST /api/imports/{id}/publish
```

An endpoint exists only when browser modules need it. Do not create a generic API layer for server-rendered forms.

Every write endpoint uses antiforgery protection or an equivalent same-origin mechanism. Imported files are never served from executable paths.

## 11. Persistence

SQLite tables should remain explicit:

- `LearnerProfiles`;
- `ProfilePreferences`;
- `CourseEnrollments`;
- `LessonProgress`;
- `PracticeSessions`;
- `PracticeAttempts`;
- `SkillEvidence`;
- `ReviewQueueItems`;
- `ImportedPackages`;
- `ContentVersions`.

Use EF Core migrations committed to the repository. Avoid a generic key-value settings table for structured domain state. JSON columns may be used only for versioned evidence payloads or device capability snapshots.

Large notation and media files live on disk under `/data/content`, with generated safe identifiers in the database.

## 12. Active-session resilience

During practice, the browser keeps a small recoverable session state in IndexedDB:

- lesson/content version;
- selected mode and section;
- tempo;
- last completed attempt summary;
- unsent final result.

Raw continuous event recording is not cached by default. After reconnection, the client may submit a completed result idempotently using a client-generated session identifier.

## 13. Localization

- ASP.NET Core resource files cover interface chrome and validation.
- Lesson package JSON covers lesson prose.
- Domain enums are formatted through localization services, never stored as translated database values.
- German and Indonesian tests verify required key parity.
- Number, date and decimal formatting use the profile culture.
- Note naming is a separate profile preference from interface language.

## 14. PWA behavior

1.0 supports installation and cached access to the app shell and previously downloaded lesson assets. It does not promise fully offline account or import management.

Offline rules:

- clearly indicate connection state;
- allow an already prepared practice session to continue;
- queue only idempotent completed results;
- never cache private imported files in a shared browser cache;
- invalidate caches by application and content version;
- provide a visible “clear offline data” action.

## 15. Docker layout

```yaml
services:
  nadiano:
    image: ghcr.io/juloc/nadiano:${NADIANO_VERSION:-latest}
    ports:
      - "8098:8080"
    volumes:
      - nadiano_data:/data
    environment:
      ASPNETCORE_URLS: http://+:8080
      NADIANO__DATA_PATH: /data
    restart: unless-stopped

volumes:
  nadiano_data:
```

The final Compose file may include health checks and read-only hardening, but should remain small. HTTPS is configured in Caddy or another reverse proxy.

## 16. Configuration

Typed configuration sections:

- `Nadiano:DataPath`;
- `Nadiano:ContentPath`;
- `Nadiano:ImportLimits`;
- `Nadiano:Privacy`;
- `Nadiano:PracticeDefaults`.

Validate configuration at startup. Do not require secrets for the local household 1.0 mode. Future external authentication and cloud integrations use secret providers rather than committed files.

## 17. Error handling

User-facing errors must distinguish:

- unsupported browser or insecure origin;
- permission denied;
- MIDI device disconnected;
- malformed or unsupported content;
- scoring/session failure;
- persistence failure;
- temporary offline state.

The practice page must retain the learner's current section and tempo after recoverable failures.

## 18. Observability

- structured server logs with event IDs;
- request correlation identifier;
- no lesson prose or raw MIDI payloads in normal logs;
- `/health/live` and `/health/ready`;
- optional diagnostics export containing versions, browser capability results and sanitized errors;
- client errors submitted only with explicit privacy-safe fields.

## 19. Security baseline

- HTTPS required externally;
- restrictive Content Security Policy compatible with self-hosted assets;
- no CDN runtime dependencies;
- XML external entities disabled;
- archive traversal and decompression-bomb protection;
- file type, count and size limits;
- generated storage names;
- non-root container;
- secure response headers;
- no arbitrary HTML in lesson translations;
- dependency and container scanning in CI.

## 20. Accessibility

- complete keyboard operation independent of the MIDI keyboard;
- visible focus states;
- semantic headings, buttons and form labels;
- screen-reader summaries for notation tasks where practical;
- text alternatives for technique media;
- reduced-motion support;
- non-color indicators for feedback;
- scalable score and interface;
- metronome visual option;
- no required audio-only instruction.

## 21. Testing architecture

### Unit tests

- event normalization;
- note/chord matching;
- sustain handling;
- timing categories;
- progression and review scheduling;
- content validation.

### Fixture tests

Recorded JSON MIDI sequences cover:

- clean scale;
- repeated notes;
- note-on velocity zero;
- early/late notes;
- rolled and simultaneous chords;
- sustain pedal overlap;
- disconnect mid-session;
- duplicate or out-of-order messages.

### Integration tests

- migrations and SQLite behavior;
- package loading;
- import security;
- localized page handlers;
- idempotent session completion.

### Browser tests

Use a fake MIDI adapter injected behind the same TypeScript interface. Real USB MIDI is a manual release test, not a CI dependency.

## 22. Version policy

- .NET runtime follows the current selected LTS and is updated deliberately.
- JavaScript dependencies are pinned by lock file.
- Content schema and application version are independent.
- Database migrations support direct upgrade from the previous stable release and documented backup/restore for older versions.
- Container tags include semantic version and immutable commit SHA.

## 23. Architecture change rule

Any change introducing a new framework, deployment service, persistence engine, public account system or content execution capability requires an architecture decision record before implementation.
