# Nadiano development rules

These rules apply to every human and automated contributor.

## Priorities

1. Preserve correct learning behavior and user safety.
2. Keep the architecture simple, testable and understandable by a junior developer.
3. Prefer direct implementation over wrappers, duplicate abstractions or temporary workarounds.
4. Keep German and Indonesian behavior equivalent.
5. Keep documentation, schemas, tests and implementation synchronized.

## Architecture

- Maintain a modular monolith.
- Use ASP.NET Core Razor Pages for server-rendered pages and small TypeScript modules for browser-only behavior.
- Browser MIDI processing stays in the browser. The Docker host does not access the learner's USB device.
- Domain logic must not depend on Razor Pages, EF Core, browser APIs or OpenSheetMusicDisplay.
- Add a dependency only when native platform features or existing dependencies cannot solve the requirement clearly.
- Do not introduce microservices, MediatR, CQRS frameworks, a message bus, a generic repository layer or a frontend SPA framework without an approved architecture decision.

## Code quality

- Follow current Microsoft C# conventions and nullable reference type rules.
- Use explicit, descriptive names. Avoid abbreviations outside established MIDI and music terms.
- Keep methods focused and small enough to understand without scrolling through unrelated behavior.
- Do not suppress warnings unless the reason is documented next to the suppression.
- Avoid reflection and dynamic behavior in core learning and scoring paths.
- All scoring must be deterministic for a fixed input event sequence and configuration.

## Learning and feedback

- Never claim MIDI can detect posture, tension or the actual finger used.
- Separate objective measurements from self-assessment prompts.
- Do not reduce a practice result to one unexplained score.
- Feedback must identify the category, location and next action.
- Introduce one physical technique cue per practice pass unless the lesson explicitly teaches cue selection.
- Every lesson declares prerequisites, goals, expected duration and completion criteria.

## Content

- MusicXML/MXL is the canonical notation exchange format.
- Bundled and imported lessons use the same schemas and validators.
- Do not commit copyrighted modern editions, recordings or copied textbook explanations.
- Use original exercises, licensed material or verified public-domain compositions with Nadiano's own engraving and fingering.
- All user-facing text uses localization keys. Do not place German or Indonesian prose in business logic.

## Testing

Every feature requires the relevant subset of:

- unit tests for domain and scoring behavior;
- recorded MIDI fixtures for timing and chord cases;
- integration tests for persistence and lesson loading;
- browser tests for the main learner path;
- schema validation tests for bundled content;
- accessibility checks for changed interactive screens.

A test may not depend on a real MIDI keyboard. Real-device checks belong to the manual release checklist.

## Changes

Before implementation:

1. Read the related documents in `docs/`.
2. Identify the smallest complete vertical slice.
3. Add or update acceptance criteria.

Before completion:

1. Run format, build and tests.
2. Update documentation and localization resources.
3. Check migrations and backup compatibility.
4. Record deliberate architecture changes in `docs/decisions/`.

## Definition of done

A task is done only when behavior, tests, error handling, localization, accessibility and documentation are complete. A screen that only works with ideal input or a feature hidden behind manual database edits is not done.
