# ADR-0003: Use MusicXML as canonical notation exchange

- Status: Accepted
- Date: 2026-08-04

## Context

Nadiano must render conventional notation, preserve parts and voices, display fingering and later import user material. MIDI represents performance events but does not reliably preserve reviewed notation, enharmonic spelling, voices or fingering.

## Decision

Use MusicXML/MXL as the canonical notation exchange format. Generate a versioned normalized expected-event document for scoring. Preserve original imported files and require review before publication as a lesson.

Bundled and private content use the same package schemas and validators.

## Consequences

- interoperability with notation software;
- standardized fingering representation;
- deterministic scoring input can be generated and tested separately;
- some MusicXML features require explicit unsupported warnings;
- a full notation editor is outside the beta scope;
- raw MIDI import needs a later quantization and notation-review workflow.

## Alternatives considered

- MIDI as canonical content: rejected because notation and fingering information are insufficient.
- Custom notation format only: rejected because it would create unnecessary conversion and editor work.
- Rendered SVG/PDF as canonical content: rejected because it lacks structured musical events.

## Reconsideration triggers

- MusicXML cannot represent a required reviewed learning construct;
- another open interoperable standard becomes materially more suitable;
- round-trip editing requirements justify an additional internal model while MusicXML remains an exchange format.
