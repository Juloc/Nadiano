# Nadiano active backlog

`MASTER_PLAN.md` is the canonical source for product scope, learning design, UI/UX, architecture, priorities and version planning.

This file intentionally contains **only unfinished active work and deliberate non-goals**. Completed 1.0/1.0.4 work is not repeated here as if it were still pending.

Current stable release: **1.0.4**.

## Status legend

| Status | Meaning |
|---|---|
| P0 | next core work; blocks the intended 1.1 learning experience |
| P1 | important product depth after/alongside P0 |
| P2 | useful refinement, not required for the next core milestone |
| Later | intentionally deferred |
| Do not build | deliberate product/architecture exclusion |

---

# P0 — 1.1 daily learning and adaptive practice

## Today session composer

Build the complete 10/20/30-minute daily plan from existing review, recommendation, repertoire and skill evidence.

Required:

- one Start/Continue action;
- warm-up/review/new skill/repertoire/sight-or-ear mix;
- reason for every selected task;
- deterministic behavior for fixed evidence and duration;
- resumable interrupted session;
- skipping without punitive language;
- no duplicate recommendation architecture.

## Adaptive micro-practice

When a learner repeatedly fails a local passage, create targeted practice instead of only another full-piece attempt.

Required:

- smallest useful section, normally one/two measures;
- dominant error category;
- one primary intervention;
- slower tempo, hands separate, rhythm-only or listen/copy where appropriate;
- mastery threshold;
- reintegration into surrounding measures;
- delayed review;
- explain why the intervention was selected.

## Progressive removal of learning aids

Introduce explicit help levels for eligible content:

- note names;
- keyboard position hints;
- fingering;
- stronger note/current-position highlighting;
- introductory timing tolerance where pedagogically justified.

Reduce aids only after sufficient evidence. Keep learner/accessibility override where necessary. Never change aid level unexpectedly in the middle of an attempt.

## Skill-specific progress refinement

Maintain and present separate evidence for:

- note reading;
- sight reading;
- rhythm;
- timing/steadiness;
- ear training;
- technique curriculum;
- chords/accompaniment;
- repertoire.

Do not replace this with one generic level.

## Adaptive sight-reading level

Add explicit difficulty dimensions and tested up/down adjustment for unseen material:

- range;
- interval size;
- rhythm complexity;
- accidentals;
- chord density;
- hand-position changes;
- hands separate/together;
- tempo;
- articulation complexity.

Only the first unseen attempt counts as fresh sight-reading evidence.

## Recommendation explanations

Every adaptive recommendation must answer why it was selected from actual evidence. Core progression remains deterministic and testable.

---

# P1 — repertoire, chords and practical playing

## Songs refinement

Add remaining high-value library behavior:

- favorites;
- difficulty filter;
- skill filter;
- style/genre filter where metadata exists;
- expected learning-time filter;
- hand-complexity filter;
- recommended-for-current-level section;
- current-piece section;
- clear original/public-domain/private source state.

Bundled and imported music continue to share one Songs surface and one Practice runtime.

## Dedicated chord/accompaniment path

Suggested progression:

1. major/minor triads;
2. chord symbols;
3. root position;
4. inversions;
5. left-hand chord patterns;
6. broken chords;
7. reusable accompaniment patterns;
8. lead-sheet reading;
9. simple pop accompaniment;
10. common progressions;
11. chord/ear integration;
12. introductory improvisation.

Reuse shared theory, ear and rhythm skills rather than duplicating the full course.

## Lead-sheet practice

Teach melody + chord symbols as a separate practical skill with objective MIDI evidence where possible.

## Imported-piece targeted practice

Generate deterministic practice suggestions from MusicXML/MXL imports:

- difficult-measure loops;
- hands separate;
- tempo ladder;
- rhythm-only drill;
- first-use sight-reading classification where appropriate;
- review scheduling.

Do not add a separate simplified import practice engine.

## Backing/accompaniment tracks

Add only original or appropriately licensed tracks where they materially improve timing/musical context.

## Richer technique demonstrations

Add original hand/fingering demonstrations and slow motion where useful. Provide text alternatives. Do not claim that MIDI verified the physical movement shown.

## Weekly and error-trend progress

Show trends only when enough evidence exists. Prefer simple lines/bars/recent-evidence views over ambiguous radar charts.

---

# P2 — refinement

## Motivation

Optional, restrained:

- flexible daily goal;
- personal challenges;
- non-punitive streak;
- meaningful achievements;
- generic XP only if it remains secondary to skill evidence.

## Practice visual refinement

Potential:

- optional dim/dark focus treatment;
- subtle state transitions;
- more tablet ergonomics testing;
- printable/large-score layouts.

Do not create a second styling architecture.

## Local audio self-review

Later 1.x candidate:

- explicit opt-in;
- local by default;
- listen-back;
- clear deletion;
- no hidden cloud upload;
- distinguish subjective sound review from objective MIDI metrics.

## Teacher notes/assignments

Later, after the household self-learning experience is mature and permission/privacy rules are clear.

---

# Manual acceptance track

These are not software backlog items and must not be falsely marked complete by CI:

- German first-run to first result in current Chrome with real permissions;
- Indonesian first-run to first result in current Edge with real permissions;
- PWA install + MIDI reconnect on production HTTPS;
- manual keyboard-only review;
- human musical/pedagogical/localization/licensing sign-off;
- sustained household/invited-user usage without manual database repair.

See `RELEASE_1_0_CHECKLIST.md` for current evidence.

---

# Deliberate non-goals

Do not build as core Nadiano patterns:

- falling-note/Guitar-Hero lane as default practice;
- posture/tension/finger correctness from MIDI;
- global leaderboard;
- loot boxes/random rewards;
- aggressive streak punishment;
- generic AI chat as scoring/progression authority;
- mandatory cloud service;
- live video conferencing;
- multiplayer/group-performance synchronization;
- full professional notation editor in 1.x;
- microservices, CQRS/MediatR, event bus or SPA framework without demonstrated need.

Later research only:

- microphone note detection for acoustic pianos;
- camera-assisted self-review;
- synchronized multi-device profiles;
- teacher/learner online accounts;
- assisted MIDI-to-MusicXML;
- licensed/community content distribution.

---

# Implementation order

1. Today session composer.
2. Adaptive micro-practice.
3. Progressive assistance fading.
4. Skill progress + adaptive sight reading.
5. Recommendation explanations/refinement.
6. Songs favorites/advanced filters.
7. Chord/accompaniment vertical slice.
8. Imported-piece practice intelligence.
9. Technique/backing-track refinements.
10. 1.1 release hardening and manual focused UX review.

For exact learner rules, UI specification, architecture constraints, work packages and release gates, use `MASTER_PLAN.md`.