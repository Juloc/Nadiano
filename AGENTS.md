# Nadiano development rules

These rules apply to every human and automated contributor.

## Plan authority

Before product or implementation work, read `docs/MASTER_PLAN.md` first.

`docs/MASTER_PLAN.md` is authoritative for current/future scope, learning behavior, UI/UX direction, priorities and version planning.

Supporting documents provide detail/evidence but must not redefine the master plan independently. If a supporting plan conflicts with the master plan, correct the conflict in the same change.

Historical Alpha/Beta checklists and known-limitations files describe those historical versions only.

## Priorities

1. Preserve correct learning behavior and user safety.
2. Keep the architecture simple, testable and understandable by a junior developer.
3. Prefer direct implementation over wrappers, duplicate abstractions or temporary workarounds.
4. Keep German and Indonesian behavior equivalent.
5. Keep documentation, schemas, tests and implementation synchronized.
6. Do not falsely mark manual hardware/human acceptance gates as automated passes.

## Architecture

- Maintain a modular monolith.
- Use ASP.NET Core Razor Pages for server-rendered pages and small TypeScript modules for browser-only behavior.
- Browser MIDI processing stays in the browser. The Docker host does not access the learner's USB device.
- Domain logic must not depend on Razor Pages, EF Core, browser APIs or OpenSheetMusicDisplay.
- Add a dependency only when native platform features or existing dependencies cannot solve the requirement clearly.
- Do not introduce microservices, MediatR, CQRS frameworks, a message bus, a generic repository layer or a frontend SPA framework without an approved architecture decision.
- Do not create a second practice/scoring engine for imported content.
- Do not make an external cloud service mandatory for the core learning loop.

## Product/navigation baseline

Current stable learner navigation is:

**Today | Learn | Songs | Train | Progress**

Profile, settings, language, MIDI setup and diagnostics are secondary.

Do not reintroduce the old peer navigation (`Home`, separate `Practice`, separate `Library`, etc.) as the main information architecture unless the master plan is deliberately changed.

## Code quality

- Follow current Microsoft C# conventions and nullable reference type rules.
- Use explicit, descriptive names. Avoid abbreviations outside established MIDI and music terms.
- Keep methods focused and small enough to understand without scrolling through unrelated behavior.
- Do not suppress warnings unless the reason is documented next to the suppression.
- Avoid reflection and dynamic behavior in core learning and scoring paths.
- All scoring/adaptive rules must be deterministic for a fixed input event sequence, state and configuration unless the master plan explicitly approves another model.

## Learning and feedback

- Standard notation remains primary for score learning.
- Never claim MIDI can detect posture, tension or the actual finger used.
- Separate objective measurements from self-assessment prompts.
- Do not reduce a practice result to one unexplained score.
- Feedback must identify category, location and next action.
- Introduce one physical technique cue per practice pass unless the lesson explicitly teaches cue selection.
- Every lesson declares prerequisites, goals, expected duration and completion criteria.
- Learning aids should be temporary where the curriculum supports fading them.
- Core recommendations must remain explainable.

## Content

- MusicXML/MXL is the canonical notation exchange format.
- Bundled and imported lessons use the same schemas, validators and runtime practice path.
- Do not commit copyrighted modern editions, recordings or copied textbook explanations.
- Use original exercises, licensed material or verified public-domain compositions with Nadiano's own engraving/fingering where appropriate.
- All user-facing interface text uses localization resources. Do not place German or Indonesian prose in business logic.
- Generated pedagogical content must follow reviewed deterministic templates/rules and validation requirements.

## MIDI boundaries

Objective MIDI evidence may include:

- pitch;
- note-on/off timing;
- velocity;
- supported controller events including Sustain CC64, Sostenuto CC66 and Soft CC67.

MIDI must not be used to claim automatic verification of posture, wrist/arm position, muscle tension or actual finger choice.

## Testing

Every feature requires the relevant subset of:

- unit tests for domain, scoring and adaptive behavior;
- recorded MIDI fixtures for timing/chord/pedal cases;
- integration tests for persistence and lesson loading;
- browser tests for the main learner path;
- schema validation tests for bundled content;
- accessibility checks for changed interactive screens;
- migration/backup/restore checks when persisted state changes.

A test may not depend on a real MIDI keyboard. Real-device checks belong to the manual release checklist.

## UI/UX rules

- Keep the interface calm, notation-first and adult-friendly.
- Practice should give most useful space to the score on landscape tablet/desktop.
- Important states should normally have one visually dominant next action.
- Do not communicate correctness through color alone.
- Keep visible keyboard focus and practical touch targets.
- Respect `prefers-reduced-motion`.
- Do not use falling-note lanes as the default practice experience.
- Avoid global leaderboards, loot boxes, aggressive streak punishment and routine confetti.
- Do not create separate desktop/mobile UI architectures.

## Changes

Before implementation:

1. Read `docs/MASTER_PLAN.md` and the relevant supporting documents.
2. Identify the smallest complete vertical slice.
3. Add/update exact acceptance criteria.
4. Confirm whether the work changes persisted state, content schema, privacy, licensing or release compatibility.

Before completion:

1. Run format, build and relevant tests.
2. Update documentation and localization resources.
3. Check migrations and backup compatibility.
4. Record deliberate architecture changes in `docs/decisions/`.
5. Update the master plan if product behavior/scope changed.

## Definition of done

A task is done only when behavior, tests, error handling, localization, accessibility and documentation are complete.

A screen that only works with ideal input, a feature hidden behind manual database edits, an adaptive rule that cannot explain its decision, or a manual gate represented as an automated success is not done.