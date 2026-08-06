# Nadiano

Nadiano is a browser-based piano learning application for two or more learner profiles. It connects to a digital piano through USB MIDI and teaches reading, rhythm, ear training, technique, repertoire and structured practice.

The interface and course content support German (`de`) and Indonesian (`id`). The localization system is designed for additional languages without duplicating lesson logic or notation files.

## Product goal

Nadiano helps a complete beginner build correct foundations and continue into selected early-intermediate material. It does not replace a qualified teacher for posture, tension, movement quality or professional interpretation. MIDI is used only for objectively measurable feedback such as pitch, onset, duration, velocity and pedal events.

## Nadiano 1.0

- complete F0, F1, B1 and B2 path plus selected E1 foundations;
- 110 guided bilingual lessons and 220 deterministic exercises;
- reading, rhythm, ear training, technique, expression, basic pedal, repertoire and practice planning;
- Web MIDI practice and three-pedal diagnostics;
- profile-private MusicXML/MXL import with hand, voice and fingering controls;
- adaptive review, session planning, PWA installation and offline result recovery;
- separate profiles with complete progress export and deletion of profile-private files;
- documented backup, restore, upgrade and rollback;
- reproducible scanned Docker release.

## Documentation

- [Product concept](docs/PRODUCT_CONCEPT.md)
- [Learning curriculum](docs/LEARNING_CURRICULUM.md)
- [Lesson and content design](docs/CONTENT_MODEL.md)
- [Technical architecture](docs/TECHNICAL_ARCHITECTURE.md)
- [Roadmap](docs/ROADMAP.md)
- [Quality and release requirements](docs/QUALITY_AND_RELEASE.md)
- [First run and browser support](docs/FIRST_RUN_AND_BROWSER_SUPPORT.md)
- [Backup and restore](docs/BACKUP_AND_RESTORE.md)
- [Upgrade to 1.0](docs/UPGRADE_TO_1.0.md)
- [Privacy](docs/PRIVACY.md)
- [1.0 release checklist](docs/RELEASE_1_0_CHECKLIST.md)
- [Research basis and sources](docs/RESEARCH_BASIS.md)
- [Architecture decisions](docs/decisions/README.md)

## Stack

- ASP.NET Core 10 Razor Pages
- TypeScript browser modules
- Web MIDI API
- OpenSheetMusicDisplay for MusicXML rendering
- Web Audio API for metronome and reference playback
- EF Core with SQLite
- PWA shell and IndexedDB for resilient practice sessions
- one Linux Docker container plus one persistent data directory

The architecture is intentionally a modular monolith. Microservices, distributed messaging and a separate frontend framework are out of scope until a demonstrated requirement exists.

## Run with Docker

Use `ghcr.io/juloc/nadiano:1.0.1`, expose container port `8080`, mount persistent storage at `/data`, and use the bundled content at `/app/content`.

## Repository rules

Read [AGENTS.md](AGENTS.md) before making changes. Documentation and implementation must remain consistent. A feature is incomplete when its behavior, tests, content schema or user-facing documentation is missing.
