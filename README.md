# Nadiano

Nadiano is a browser-based piano learning application for two or more learner profiles. It connects to a digital piano through USB MIDI and teaches reading, rhythm, ear training, technique, repertoire and structured practice.

The initial interface and course content support German (`de`) and Indonesian (`id`). The localization system is designed for additional languages without duplicating lesson logic or notation files.

## Product goal

Nadiano should help a complete beginner build correct foundations and continue through intermediate and advanced study. It does not replace a qualified teacher for posture, tension, movement quality or professional interpretation. MIDI is used only for objectively measurable feedback such as pitch, onset, duration, velocity and pedal events.

## Delivery stages

- **Alpha:** working vertical slice for internal use: profiles, MIDI setup, notation, metronome, guided practice, deterministic scoring and the first foundation lessons.
- **Beta:** complete beginner path, adaptive practice, progress tracking, MusicXML import, German and Indonesian content, installation as a PWA and reliable Docker deployment.
- **1.0:** polished self-hosted product with a complete beginner curriculum, stable content format, privacy controls, backup/restore, accessibility, automated release validation and documented extension points.

## Documentation

- [Product concept](docs/PRODUCT_CONCEPT.md)
- [Learning curriculum](docs/LEARNING_CURRICULUM.md)
- [Lesson and content design](docs/CONTENT_MODEL.md)
- [Technical architecture](docs/TECHNICAL_ARCHITECTURE.md)
- [Junior implementation plan](docs/JUNIOR_IMPLEMENTATION_PLAN.md)
- [Alpha, beta and 1.0 roadmap](docs/ROADMAP.md)
- [Quality and release requirements](docs/QUALITY_AND_RELEASE.md)
- [First run and browser support](docs/FIRST_RUN_AND_BROWSER_SUPPORT.md)
- [Backup and restore](docs/BACKUP_AND_RESTORE.md)
- [Beta release checklist](docs/BETA_RELEASE_CHECKLIST.md)
- [Beta known limitations](docs/KNOWN_LIMITATIONS_BETA.md)
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
- one Linux Docker container plus one persistent data volume

The architecture is intentionally a modular monolith. Microservices, distributed messaging and a separate frontend framework are out of scope until a demonstrated requirement exists.

## Repository rules

Read [AGENTS.md](AGENTS.md) before making changes. Documentation and implementation must remain consistent. A feature is incomplete when its behavior, tests, content schema or user-facing documentation is missing.

## Status

`0.2.0-beta.1` contains the complete planned B1/B2 functional path: 45 guided Beta lessons, 100 deterministic exercises, review scheduling, adaptive practice, secure private MusicXML/MXL import, PWA installation, offline result recovery and separate diagnostics for three standard piano pedal controllers. Real-piano and two-week household testing remain release evidence, not hidden assumptions.
