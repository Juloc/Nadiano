# Nadiano UI/UX status and remaining backlog

`MASTER_PLAN.md` is the canonical UI/UX specification.

This file no longer describes the pre-1.0.4 interface as the current product. The structural redesign documented here was implemented in **1.0.4** and is now the stable baseline.

## Research basis

The redesign direction was informed by current piano-learning products and established teaching patterns, including:

- Simply Piano — simple step-by-step entry and obvious next action;
- flowkey — score-focused piece practice, Wait, loop, tempo and hand controls;
- Skoove — short guided lesson structure and immediate practice;
- Piano Marvel — structured technique/sight-reading/progress tools;
- Yousician-style products — useful immediacy/gamification ideas, but also patterns Nadiano deliberately avoids such as over-reliance on game-like visual lanes.

Research is design input only. Nadiano keeps its own notation-first, calm and adult-friendly identity.

---

# Implemented in 1.0.4

## Information architecture

Implemented:

- exactly five primary learner destinations: Today, Learn, Songs, Train, Progress;
- profile/settings/language/MIDI/diagnostics moved to secondary controls;
- consistent phone navigation with accessible SVG icons;
- focused Practice hides unrelated learner navigation.

## Today

Implemented baseline:

- real learner progress instead of static placeholders;
- due reviews;
- recommended next lesson;
- course completion context;
- clear learner-facing primary action structure.

Still planned:

- complete 10/20/30-minute session composer;
- visible composed session outline;
- resumable daily-session state;
- per-task recommendation reason.

## Learn

Implemented baseline:

- distinct curriculum/course purpose;
- current/recommended learning context;
- cleaner learner hierarchy than the old peer-card layout.

Still planned/refinement:

- richer unit mastery/review display where evidence supports it;
- chord/accompaniment emphasis once that path exists;
- user testing of unit density and long-course navigation.

## Songs

Implemented baseline:

- bundled and private/imported music in one surface;
- same Practice route/runtime;
- text search;
- source filter;
- readiness/status filter.

Still planned:

- favorites;
- difficulty/skill/style/time/hand-complexity filters;
- recommended/current-piece sections;
- richer source/progress metadata.

## Train

Implemented baseline:

- separate learner-facing training destination;
- clearer skill-oriented presentation than the old generic training surface.

Still planned:

- stronger per-skill recommendation state;
- adaptive sight-reading level presentation;
- chord/accompaniment modules as the curriculum expands.

## Progress

Implemented baseline:

- distinct progress surface tied to existing evidence/recommendation data;
- learner-facing hierarchy rather than technical card equality.

Still planned:

- richer separate skill levels;
- sight-reading evidence;
- weekly/error trends when enough data exists;
- clearer review-load explanation.

## Practice

Implemented baseline:

- score-dominant workspace on larger displays;
- compact practice controls;
- mode/tempo/hand/loop/zoom/start-stop controls without the old long vertical form;
- secondary metronome/reference/fullscreen tools;
- compact MIDI state;
- quieter live feedback;
- cursor/problem-location focus;
- result prioritizes first problem measures + concrete next action;
- source-aware return to Learn or Songs.

Still planned/refinement:

- adaptive micro-practice flow integrated directly from result;
- aid-level/fingering fading;
- additional post-attempt score markers/details where useful;
- more tablet ergonomics/user testing;
- optional dim focus treatment only if it remains one design system.

## Setup/MIDI

Implemented:

1. browser/security check;
2. explicit permission action;
3. device selection;
4. real key/pedal test;
5. completion;
6. separate Sustain CC64, Sostenuto CC66 and Soft CC67 detection;
7. raw diagnostics behind secondary disclosure;
8. recoverable device reselection.

---

# Stable visual rules

These are no longer open research questions; they are product rules.

## Instrument first

Use screen space for notation and immediate learning information before decoration. The physical piano and the score are the primary focus while playing.

## Calm, modern, adult-friendly

Desired:

- clean;
- precise;
- restrained;
- friendly without childish visual treatment;
- clear hierarchy;
- moderate whitespace;
- limited saturated color.

Avoid:

- cartoon mascot as core identity;
- confetti-heavy routine success;
- game-map scenery;
- neon piano decoration;
- excessive gradients/glass;
- equal-weight card soup.

## One primary action

Important learner states should normally expose one visually dominant next action.

## Progressive disclosure

Show essential controls first. Advanced/mode-specific settings appear only when relevant. Diagnostics remain secondary.

## Notation first

Standard notation remains the primary score-learning visual. Falling-note/Guitar-Hero lanes are not the default practice model.

## Semantic color

Use semantic tokens for canvas/surface/text/border/accent/focus/success/warning/error/current. Correctness must never rely on red/green alone.

## Light default

Light remains the default UI because of score readability. A dim/dark practice treatment is optional later, not a separate parallel theme architecture.

## Typography

Use one highly legible system/Segoe-style sans-serif stack. Hierarchy comes from size/weight/spacing, not multiple decorative fonts.

## Motion

Short, functional state transitions only. No continuous decoration. Always respect `prefers-reduced-motion`.

---

# Responsive rules

Primary score-practice targets:

1. landscape tablet on music stand;
2. laptop/desktop near piano;
3. large desktop monitor.

Phone remains strong for Today, Learn, Songs, Train, ear/rhythm work, progress and setup. Dense full-grand-staff practice remains functional but is not treated as equally comfortable on every phone.

Use behavior-based breakpoints when navigation, score width or controls stop fitting cleanly.

---

# Accessibility rules

Required for all refinements:

- practical ~44px touch targets where possible;
- visible keyboard focus;
- no essential hover-only behavior;
- semantic headings/landmarks/labels;
- non-color feedback cues;
- reduced motion;
- score zoom/scaling;
- text alternatives for technique media;
- no unexpected sound autoplay;
- keyboard operation independent of MIDI keyboard.

---

# Remaining UI priorities

## P0

1. Today 10/20/30-minute composed session flow.
2. Micro-practice result → targeted section flow.
3. Assistance/fingering fade states.
4. Skill-specific Progress and adaptive Sight Reading UI.

## P1

5. Songs favorites and advanced filters.
6. Chord/accompaniment path presentation.
7. Imported-piece generated-practice presentation.
8. Weekly/error trends with sufficient evidence.
9. Tablet ergonomics/user testing and measured refinements.

## P2

10. Optional dim focus mode.
11. Subtle micro-interactions.
12. Printable/large-score layouts.
13. Richer original technique demonstration layouts.
14. Optional local audio self-review UI if/when that feature is approved.

The complete rules, reasons, work packages and version targets are in `MASTER_PLAN.md`.