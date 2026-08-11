# Nadiano roadmap

`MASTER_PLAN.md` is the canonical source for complete product scope, pedagogy, UI/UX, architecture and work-package detail.

This file contains only the **current version sequence and milestone gates** so it does not compete with the master plan.

Current stable release: **1.0.4**.

---

# Completed foundation

The following milestones are complete as software baselines and remain visible in git/release history:

- A0 — repository/delivery foundation;
- A1 — MIDI/audio foundation;
- A2 — notation/deterministic scoring;
- Alpha — first complete household learning loop;
- B1 — complete beginner course structure;
- B2 — import, review scheduling and PWA path;
- Beta — feature-complete 1.0 candidate;
- RC/1.0 hardening — backup/restore, accessibility, performance, security and release gates;
- 1.0.0–1.0.3 — stable content/operations/accessibility/performance releases;
- **1.0.4 — learner-focused UI/UX structural redesign.**

Historical Alpha/Beta scope remains documented in the versioned release checklists and limitation documents; it is not future work.

---

# 1.0.4 — current stable baseline

Released baseline includes:

- F0/F1/B1/B2 plus selected introductory E1 content;
- required 1.0 content quantities and validated repertoire;
- stable MIDI/scoring/practice engine;
- three-pedal diagnostics for CC64/66/67;
- private MusicXML/MXL import/export path;
- review scheduling and skill evidence;
- Today, Learn, Songs, Train, Progress navigation;
- unified bundled/private Songs surface;
- score-dominant focused Practice workspace;
- problem-location + next-action results;
- progressive MIDI onboarding;
- PWA/offline result recovery baseline;
- non-root Docker deployment;
- automated frontend/.NET/content/browser/accessibility/security gates;
- 1 CPU / 512 MiB performance gate;
- upgrade, cold-restore and rollback rehearsals.

## Manual acceptance track still open

- German real-permission Chrome first-run → result;
- Indonesian real-permission Edge first-run → result;
- production HTTPS PWA install + MIDI reconnect;
- manual keyboard-only review;
- human musical/pedagogical/localization/licensing sign-off;
- sustained household/invited-user daily-use evidence without manual DB repair.

These remain manual evidence and must not be claimed by CI.

---

# 1.0.x patch policy

Patch releases after 1.0.4 are limited to:

- defect correction;
- security/compatibility fixes;
- accessibility corrections;
- contained UX refinements;
- documentation/release evidence corrections.

New learning-model capabilities belong in 1.1+.

---

# 1.1 — Daily learning and adaptive practice

## Goal

Turn the stable 1.0.4 surfaces into a stronger personalized daily practice system while keeping the recommendation engine deterministic and explainable.

## Scope

- full Today session composer for 10/20/30-minute plans;
- resumable daily session;
- review/new-skill/repertoire/sight-or-ear balancing;
- per-task recommendation reason;
- adaptive one/two-measure micro-practice;
- tempo/hands/rhythm/listen interventions;
- mastery + reintegration + delayed review;
- progressive removal of learning aids;
- richer separate skill evidence;
- adaptive sight-reading difficulty;
- stronger recommendation explanations;
- Songs favorites and richer filters/recommendations;
- focused tablet ergonomics/user testing.

## Exit criteria

- one action starts a useful evidence-based daily plan;
- plans fit selected 10/20/30-minute budgets without duplicating recommendation logic;
- recurring localized errors produce targeted section practice;
- mastery interventions are deterministic and explainable;
- learning aids fade only with sufficient evidence and remain accessibility-safe;
- fresh sight-reading difficulty adjusts through tested explicit rules;
- German/Indonesian parity remains complete;
- all standard stable release gates pass;
- migration/backup/restore/rollback compatibility is verified.

---

# 1.2 — Chords, accompaniment and imported-piece intelligence

## Goal

Make Nadiano strong for practical accompaniment and learner-owned repertoire, not only fully notated solo playing.

## Scope

- dedicated chord/accompaniment path;
- major/minor triads;
- chord symbols;
- inversions;
- left-hand/broken-chord patterns;
- reusable accompaniment patterns;
- lead-sheet reading;
- simple pop accompaniment;
- common progression recognition/application;
- introductory improvisation;
- deterministic targeted-practice suggestions from MusicXML/MXL imports;
- richer repertoire metadata;
- original/licensed backing tracks where useful.

## Exit criteria

- chord path reuses shared rhythm/theory/ear skills rather than duplicating the curriculum;
- at least one complete lead-sheet/accompaniment vertical slice is validated before catalogue expansion;
- imported and bundled content still use the same runtime path;
- generated imported-piece practice is deterministic and explainable;
- content/licensing gates remain satisfied;
- all stable release gates pass.

---

# 1.3 — Intermediate expansion and richer self-review

## Direction

Potential 1.3 work, to be split into approved vertical slices:

- expanded E1/intermediate curriculum;
- improved pedal and dynamics analysis;
- richer original technique/hand demonstrations;
- printable/large-score layouts;
- additional notation naming options/languages;
- optional local audio recording and listen-back;
- teacher notes on private profiles if permissions remain simple.

No item is automatically included merely because it is listed here; each needs scope, acceptance criteria, tests, privacy/licensing review and an implementation issue/work package.

---

# 2.0 candidates

These are research candidates, not promises:

- assisted MIDI-to-MusicXML workflow;
- teacher/learner online account model;
- synchronized multi-device profiles;
- optional camera-assisted self-review;
- richer notation editing;
- licensed/community content distribution.

They require new architecture/privacy/licensing decisions before implementation.

---

# Permanent non-goals unless the master plan changes

- MIDI-only posture/tension/finger correctness;
- falling-note lane as the default learning model;
- global leaderboards/loot boxes/aggressive streak punishment;
- generic AI chat as scoring/progression authority;
- mandatory cloud service for core learning;
- live video conferencing as a core feature;
- multiplayer synchronization as a priority;
- full professional notation editor in 1.x;
- microservices/CQRS/MediatR/event bus/SPA framework without demonstrated need.

For exact behavior, UI rules, active backlog, work packages and definitions of done, use `MASTER_PLAN.md`.