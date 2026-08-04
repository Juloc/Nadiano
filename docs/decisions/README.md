# Architecture decisions

Architecture decisions record choices that constrain future implementation. They explain context, decision, consequences and conditions for reconsideration.

## Accepted decisions

- [ADR-0001: Use a modular monolith](0001-modular-monolith.md)
- [ADR-0002: Process live MIDI in the browser](0002-browser-midi.md)
- [ADR-0003: Use MusicXML as canonical notation exchange](0003-musicxml-canonical.md)
- [ADR-0004: Use Razor Pages with focused TypeScript modules](0004-razor-pages-typescript.md)
- [ADR-0005: Use SQLite and one application container for 1.0](0005-sqlite-single-container.md)

## Required format

New decisions use the next number and contain:

- status;
- date;
- context;
- decision;
- consequences;
- alternatives considered;
- reconsideration triggers.

A new dependency alone does not always need an ADR. A new framework, service, persistence engine, public identity model, executable content format or privacy boundary does.
