# Junior implementation plan — current baseline

`MASTER_PLAN.md` is the canonical source for product behavior, learning rules, UI/UX, architecture and version targets.

This file contains only **active work packages from the current stable 1.0.4 baseline**.

Historical WP-001 through WP-045 delivered the repository foundation, MIDI/audio, notation/scoring, Alpha/Beta, secure imports, PWA, content catalogue, backup/restore, accessibility, performance and stable 1.0 release path. They remain available in git history and historical release documents; they are not future tasks.

## How to use this plan

For every work package:

1. Read the relevant sections of `MASTER_PLAN.md` first.
2. Keep the smallest complete learner-facing vertical slice.
3. Define exact scope and exclusions before coding.
4. Add tests with or before behavior.
5. Keep German and Indonesian equivalent.
6. Preserve accessibility and current browser/MIDI behavior.
7. Run format/build/tests/content validation relevant to the change.
8. Update `MASTER_PLAN.md` if the intended behavior changes.
9. Do not add a new architecture layer just to implement a UI feature.

---

# WP-046 — Documentation consolidation

## Goal

Keep one authoritative plan and remove contradictory current-status documentation.

## Deliver

- `MASTER_PLAN.md` as canonical source;
- current README version/deployment information;
- Roadmap reduced to version sequence;
- Backlog reduced to unfinished work;
- UI backlog reduced to implemented baseline + remaining refinements;
- Handover updated from Alpha-era state;
- contributor rules point to the master plan.

## Acceptance criteria

- no active planning document claims the old pre-1.0.4 navigation is current;
- no active planning document says UI research is still missing;
- current stable version is 1.0.4 where a current version is intended;
- historical Alpha/Beta files remain clearly versioned historical evidence;
- future contributors know which document wins on conflict.

---

# WP-047 — Today session composer

## Goal

Turn Today from a progress-aware entry page into a complete evidence-based daily practice flow.

## Scope

### Session lengths

Offer approximately:

- 10 minutes;
- 20 minutes;
- 30 minutes.

The selected length changes actual task composition/size rather than only hiding text.

### Inputs

Reuse existing sources:

- review queue;
- current recommended lesson;
- current course state;
- skill evidence;
- current repertoire state;
- sight-reading/ear/rhythm candidates.

Do not build a second recommendation store.

### Default composition

When enough material exists, prefer:

1. short warm-up;
2. due review;
3. new skill/lesson;
4. current repertoire;
5. sight-reading or ear/rhythm task;
6. summary.

Short plans may omit or combine items using explicit priority rules.

### UX

- one clear Start/Continue action;
- visible plan outline;
- approximate duration per task;
- current/upcoming/complete state;
- reason available for each selected item;
- skipping allowed without punitive language;
- interrupted plan can resume.

## Tests

- deterministic plan for fixed evidence/date/duration;
- due review is not silently dropped when it should fit;
- unrelated skill is not cleared by repertoire result;
- plan never references unavailable content;
- resume does not duplicate completed items;
- DE/ID resource parity;
- browser/accessibility path.

## Done when

A learner can open Today, choose a duration and begin a useful complete session with one action.

---

# WP-048 — Adaptive micro-practice

## Goal

Convert repeated localized errors into targeted deliberate practice.

## Detection

Use explicit evidence such as:

- same measure/beat weak across attempts;
- repeated pitch errors;
- repeated timing instability;
- hands-together degradation compared with each hand;
- rhythm instability with otherwise stable pitch;
- target-tempo failure after lower-tempo success.

## Interventions

Choose one primary action:

- smaller one/two-measure loop;
- lower tempo;
- left/right hand only;
- rhythm-only;
- listen/copy;
- re-read with temporary aid if reading is the problem.

## Mastery loop

1. start focused section;
2. run selected intervention;
3. evaluate against configured criterion;
4. repeat/change intervention only through explicit rules;
5. reinsert into surrounding measures;
6. schedule delayed review;
7. return to piece/lesson.

Do not require exactly five repetitions universally.

## Tests

- deterministic rule choice;
- no endless retry loop;
- correct location carried into Practice;
- explanation matches rule;
- intervention disabled when content/mode cannot support it;
- reintegration and delayed review state persist correctly.

---

# WP-049 — Progressive assistance fading

## Goal

Prevent permanent dependency on beginner aids.

## Supported aid types

Where content supports them:

- note names;
- keyboard-position highlights;
- fingering;
- stronger current-note highlighting;
- introductory timing tolerance.

## Rules

- define explicit aid levels;
- lesson/content declares which aids are allowed;
- evidence determines suggested level;
- learner/accessibility setting may retain an aid;
- never silently change aid level during an active attempt;
- explain why an aid was reduced when useful.

## Tests

- insufficient evidence never removes required help;
- stable evidence can reduce eligible help;
- accessibility override wins;
- content that forbids an aid never shows it;
- refresh/session resume preserves selected state correctly.

---

# WP-050 — Skill progress and adaptive sight reading

## Goal

Strengthen separate competency evidence and make sight-reading difficulty adaptive.

## Skill evidence

Maintain/present at least:

- note reading;
- sight reading;
- rhythm;
- timing/steadiness;
- ear training;
- technique curriculum;
- chords/accompaniment;
- repertoire.

## Sight-reading dimensions

- note range;
- interval size;
- rhythm complexity;
- accidentals;
- chord density;
- hand-position changes;
- hands separate/together;
- tempo;
- articulation complexity.

## Rules

- only first unseen attempt creates fresh sight-reading evidence;
- repeated same material becomes ordinary practice;
- raise/lower difficulty through explicit tested rules;
- avoid changing many dimensions at once when one is the clear limiting factor;
- explain level change in learner language.

## UI

- current sight-reading state in Train/Progress;
- preview timer and restricted assessment semantics in Practice;
- post-attempt result shows what changed and why.

---

# WP-051 — Chord/accompaniment vertical slice

## Goal

Prove the practical accompaniment path before creating a large catalogue.

## First vertical slice

Implement one coherent sequence covering:

1. one major/minor triad concept;
2. chord-symbol recognition;
3. one inversion concept;
4. one reusable left-hand accompaniment pattern;
5. one simple lead-sheet task;
6. one musical application.

## Rules

- reuse shared rhythm/theory/ear skills;
- do not duplicate the full course;
- standard notation/chord symbols remain transferable;
- MIDI scoring evaluates only measurable evidence;
- physical technique remains instruction/self-check.

## Acceptance

- learner can understand the chord, play it, recognize the symbol and use it musically;
- progress is stored under relevant shared skills;
- DE/ID content reviewed;
- no copied copyrighted method text/score.

After this slice is validated, expand toward triads, inversions, patterns, lead sheets, progressions and basic improvisation as defined by `MASTER_PLAN.md`.

---

# WP-052 — Imported-piece practice intelligence

## Goal

Make private MusicXML/MXL useful for structured Nadiano practice without a second engine.

## Scope

From parsed notation and practice evidence, propose deterministic tasks such as:

- difficult-measure loop;
- hands separate;
- tempo ladder;
- rhythm-only section;
- first-use sight-reading classification where appropriate;
- delayed review.

## Constraints

- same Practice/scoring runtime as bundled pieces;
- no automatic claim that imported fingering is correct;
- no full notation editor;
- import security limits remain unchanged;
- every generated suggestion can explain its source/rule.

---

# WP-053 — Repertoire and technique refinement

## Songs

Add remaining high-value selection features:

- favorites;
- difficulty/skill filters;
- style/genre where metadata exists;
- expected learning time;
- hand complexity;
- recommended/current sections;
- stronger source/progress metadata.

## Technique media

Where useful:

- original hand/fingering demonstration;
- slow motion;
- text alternative;
- common-correct/incorrect contrast;
- optional/collapsible presentation in lessons/practice.

## Accompaniment

Add original or appropriately licensed backing tracks only where they improve the learning task.

---

# WP-054 — 1.1 release hardening

## Goal

Release the daily/adaptive learning improvements without weakening the stable operational baseline.

## Required automated gates

- frontend build/lint/tests/audit;
- .NET format/build/tests;
- content validation;
- browser critical paths;
- accessibility baseline;
- Docker non-root build;
- Trivy Critical/High policy;
- performance profile;
- upgrade rehearsal;
- cold restore;
- rollback;
- dependency/license reports;
- immutable semantic + commit-SHA image tags.

## Additional 1.1 checks

- Today plan composition fixtures;
- micro-practice rule fixtures;
- assistance-fade fixtures;
- sight-reading adaptation fixtures;
- daily-session resume/idempotency;
- manual focused tablet/laptop practice UX review.

## Manual evidence

Do not replace real hardware/browser permission testing with fake-MIDI CI. Record remaining manual gates explicitly.

---

# Later packages

After 1.1, create new work packages for 1.2/1.3 only when the exact vertical slice is approved in `MASTER_PLAN.md`/`ROADMAP.md`.

Likely themes:

- expanded chord/accompaniment catalogue;
- imported-piece intelligence refinement;
- intermediate E1 content;
- richer pedal/dynamics analysis;
- printable/large-score layouts;
- optional local audio self-review;
- teacher notes/assignments.

2.0 research candidates such as accounts/sync, camera-assisted review, MIDI-to-MusicXML and richer notation editing require architecture/privacy decisions first.