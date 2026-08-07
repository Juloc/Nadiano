# Nadiano UI/UX backlog

This document is the detailed UI/UX backlog referenced by `BACKLOG.md` item 93. It records the post-1.0 product-design direction after reviewing the current Nadiano UI and current piano-learning products.

The goal is not to imitate one competitor. Nadiano should combine the clearest patterns while remaining notation-first, calm, adult-friendly, accessible and honest about what MIDI can measure.

## Research inputs

Current official product material reviewed in August 2026:

- Simply Piano — simple step-by-step entry, goals, courses, songs and guided play: https://www.hellosimply.com/en/simply-piano
- Simply Piano PLAY — interactive sheet music, Teach Me, feedback, auto-scroll and tempo controls: https://piano-help.hellosimply.com/en/articles/7943680-understanding-play
- flowkey — score plus pianist video, Wait Mode, slow motion, loop and hand selection: https://www.flowkey.com/en
- Skoove — bite-sized guided lessons, video guidance, moving score and real-time feedback: https://www.skoove.com/en
- Piano Marvel — dashboard, Practice/Play modes, lesson videos and skill/progress tools: https://pianomarvel.com/en/new-features
- Piano Marvel SASR — adaptive sight-reading material and progress history: https://pianomarvel.com/en/feature/sasr

Research is used as design input only. Nadiano must keep its own visual identity and learning rules.

---

# 1. Current Nadiano UI baseline

The existing 1.0 UI is functionally usable but should be treated as a stable technical baseline, not the final product design.

Current issues to address:

- the global navigation exposes Home, Learn, Beginner Course, Practice, Training, Library, Progress, Settings and Profiles at the same level;
- language selection also competes for header space;
- the global content width is capped at roughly 64rem even on large practice displays;
- generic `.card`, `.field-list` and form patterns dominate many pages;
- the practice screen is a vertical sequence of goal card, device status, notation, fingering, zoom, mode fields, start/stop controls, live feedback and result card;
- notation is present but does not yet own most of the practice workspace;
- the visual hierarchy is functional but weak: most surfaces have similar weight;
- learner navigation, administrative settings and diagnostics are not sufficiently separated;
- the visual system has only a small set of tokens and one generic radius;
- responsive rules are not yet organized around the real use cases of phone, tablet at the piano and desktop/laptop at the piano.

Do not discard the existing accessibility baseline, reduced-motion support, localization or simple architecture while redesigning the UI.

---

# 2. Product design principles

## UI-01 — Instrument first

**Decision: High priority**

The interface should disappear while the learner plays. Standard notation and the physical piano are the primary objects of attention.

Rules:

- use screen space for score and immediate learning information before decorative UI;
- keep controls close to the action they affect;
- hide unrelated navigation during focused practice;
- do not make learners repeatedly touch the screen between short passages unless necessary.

## UI-02 — Calm, modern and adult-friendly

**Decision: High priority**

Nadiano should feel like a high-quality music-learning workspace, not a children's game or enterprise dashboard.

Desired qualities:

- clean;
- calm;
- precise;
- friendly without being childish;
- visually light;
- clear hierarchy;
- restrained use of color;
- enough whitespace to reduce cognitive load.

Avoid:

- cartoon mascots as the core identity;
- confetti-heavy success states;
- game-map scenery;
- neon/glowing piano-key decoration;
- excessive gradients;
- glassmorphism for ordinary controls;
- dashboard-like card soup.

## UI-03 — Learning state before statistics

**Decision: High priority**

Every main screen should answer a learner question:

- Today: What should I practise now?
- Learn: What am I learning and what comes next?
- Songs: What can I play or learn?
- Train: Which skill do I want to sharpen?
- Progress: What is improving and what needs review?
- Practice: What should I play right now and what should I change next?

Do not lead with technical data merely because it is easy to display.

## UI-04 — One primary action per state

**Decision: Recommended**

A screen may contain several controls, but it should normally have one visually dominant next action.

Examples:

- `Start today's practice`;
- `Continue lesson`;
- `Practise difficult section`;
- `Start song`;
- `Retry at 60 BPM`.

## UI-05 — Progressive disclosure

**Decision: High priority**

Beginners should not see every advanced option at once.

- show essential controls first;
- place uncommon settings in a compact overflow/settings surface;
- reveal mode-specific controls only when relevant;
- keep diagnostics outside the normal learner flow;
- progressively remove learning aids as competency improves.

---

# 3. Primary information architecture

## UI-06 — Five primary learner destinations

**Decision: High priority**

Use exactly these primary destinations for normal learner navigation:

**Today | Learn | Songs | Train | Progress**

German UI:

**Heute | Lernen | Songs | Trainieren | Fortschritt**

Indonesian labels must remain semantically equivalent.

Secondary destinations:

- learner/profile switcher;
- settings;
- MIDI setup/status;
- diagnostics/help;
- language;
- data export/delete.

These secondary destinations must not occupy equal visual weight with the five learning destinations.

## UI-07 — Navigation by device class

**Decision: Recommended**

### Desktop and large tablet

Preferred patterns:

- compact persistent side rail/sidebar for the five destinations; or
- compact top navigation if it leaves substantially more useful room for notation.

Do not place nine or more peer links across the header.

### Phone

Use a five-item bottom navigation for the primary learner destinations.

Profile/settings may be opened from a separate profile/menu control.

### Focused practice

Hide the normal global navigation. Keep only:

- exit/back;
- piece/lesson context;
- MIDI status;
- essential practice controls.

---

# 4. Visual language

## UI-08 — Color system

**Decision: High priority**

Use a restrained token system rather than page-specific colors.

Required semantic roles:

- canvas/background;
- primary surface;
- secondary/subtle surface;
- primary text;
- secondary text;
- border/divider;
- brand/accent;
- focus;
- success;
- warning;
- error;
- current/active notation state.

Rules:

- brand accent is not automatically the success color;
- correct/wrong feedback must never rely on green/red alone;
- pair semantic colors with icons, labels, borders or shapes;
- reserve saturated color for actions, status and musical feedback;
- avoid giving every skill category a strong permanent color unless the mapping proves useful and accessible.

Exact color values should be chosen during implementation with contrast testing rather than copied from a competitor.

## UI-09 — Light first, dark/focus later

**Decision: Recommended**

Default learner UI should remain light because sheet music naturally fits a light score surface and printed-notation expectations.

A dark or dimmed **practice focus theme** may be added later if:

- the score remains highly legible;
- contrast is validated;
- semantic feedback still works;
- it does not become a parallel styling architecture.

## UI-10 — Typography

**Decision: Recommended**

Use one highly legible UI sans-serif stack. The existing system/Segoe-style direction is acceptable.

Hierarchy should be driven by size, weight and spacing rather than many fonts.

Recommended hierarchy:

- page title;
- section title;
- card/module title;
- normal body;
- supporting/meta text;
- compact labels.

Rules:

- avoid tiny low-contrast helper text;
- keep line lengths comfortable on instructional pages;
- score notation remains visually more important than surrounding prose during practice.

## UI-11 — Spacing and density

**Decision: High priority**

Use a small spacing scale consistently.

The normal application should be moderately spacious. Practice controls can be compact but must remain touch-safe.

Do not use large empty hero areas when the learner came to practise immediately.

## UI-12 — Corners, borders and elevation

**Decision: Recommended**

Use modest rounded corners and subtle borders/elevation.

Rules:

- not every section needs a card;
- group related information with spacing first;
- use cards for genuinely separate interactive/content modules;
- avoid nesting cards inside cards unless the hierarchy is necessary;
- score paper may use its own clear surface treatment.

## UI-13 — Icons

**Decision: Recommended**

Use a single simple line/filled icon family with consistent stroke/weight.

Icons should support labels, not replace unfamiliar learning terms.

Do not use emoji as permanent product icons.

## UI-14 — Motion

**Decision: Recommended, restrained**

Use motion only to clarify state changes:

- navigation/content transition;
- selected measure/loop changes;
- compact success acknowledgement;
- panel expansion;
- connection/status change.

Rules:

- short and subtle;
- never animate notation just for decoration;
- no continuous background animation;
- no mandatory celebration sequences;
- honor `prefers-reduced-motion` everywhere.

---

# 5. Today screen

## UI-15 — One obvious daily entry point

**Decision: High priority**

The Today screen should not become an analytics dashboard.

Primary structure:

1. compact greeting/context;
2. one strong `Start/Continue today's practice` action;
3. planned duration;
4. visible 3–6 step session outline;
5. due review count if relevant;
6. current piece/lesson context;
7. compact MIDI readiness state.

Optional control:

- 10 / 20 / 30 minute plan length.

Each session step should show:

- task type;
- short learner-facing purpose;
- approximate duration;
- state: upcoming / current / complete;
- why it was selected when the learner asks for details.

Avoid:

- many KPI cards above the start button;
- giant streak number;
- multiple equally prominent CTAs;
- forcing manual exercise selection before every session.

---

# 6. Learn screen

## UI-16 — Structured course path, not a fantasy game map

**Decision: High priority**

Use a clear vertical curriculum grouped into meaningful units.

Each unit should expose:

- title;
- musical goal;
- completion/mastery state;
- estimated time;
- prerequisite only when relevant;
- current recommended lesson;
- review-needed state.

Preferred presentation:

- readable vertical units/chapters;
- a visible current position;
- compact progress within each unit;
- optional branch labels for solo notation versus chord/accompaniment goals.

Do not create a decorative zig-zag map merely to appear game-like.

## UI-17 — Goal paths without duplicating the curriculum

**Decision: Recommended**

Learners may emphasize goals such as:

- notation/solo playing;
- chords/accompaniment;
- repertoire;
- ear/improvisation.

These should reuse shared skills and lessons instead of creating four separate copies of the course architecture.

---

# 7. Songs screen

## UI-18 — Library optimized for choosing music

**Decision: High priority**

Primary elements:

- search;
- Continue practising;
- favorites;
- recommended for current level;
- filters;
- bundled and private/imported music.

Useful filters:

- difficulty;
- skill;
- style/genre;
- expected learning time;
- hands/separation complexity;
- current/mastered/not started;
- original/public-domain/private.

## UI-19 — Song cards remain informative, not decorative album art

**Decision: Recommended**

A song item should prioritize:

- title;
- composer/source;
- difficulty;
- relevant skills;
- current progress;
- estimated duration;
- favorite state.

For classical/public-domain repertoire, do not invent unrelated album-cover art just to fill space. A small engraving preview or restrained artwork is acceptable where useful.

Imported MusicXML should visually fit the same library instead of looking like a separate developer tool.

---

# 8. Train screen

## UI-20 — Skill modules instead of a wall of exercises

**Decision: High priority**

Top-level training modules:

- Note reading;
- Sight reading;
- Rhythm;
- Ear training;
- Scales & chords;
- Technique;
- Pedal where measurable.

Each module should show:

- current level/state;
- one recommended next drill;
- due review when applicable;
- option to choose another drill.

Avoid showing dozens of tiny exercise tiles at once.

---

# 9. Practice workspace

This is the highest-priority UI redesign.

flowkey's strongest reusable idea is the clear focus on score plus a small set of immediate practice tools. Simply Piano's reusable idea is keeping guided actions obvious. Piano Marvel's reusable idea is explicit practice/assessment modes and structured work on smaller sections. Nadiano should combine these without introducing a game lane.

## UI-21 — Score dominates the viewport

**Decision: Critical / P0**

On landscape tablet/laptop/desktop, the score should normally receive the majority of available height and width.

The global 64rem content cap should not constrain the focused practice workspace on large displays.

The score area should support:

- readable full-grand-staff notation;
- zoom;
- cursor/current measure;
- direct measure selection;
- loop-range visualization;
- post-attempt error markers;
- auto-follow/scroll where needed.

## UI-22 — Compact persistent practice control bar

**Decision: Critical / P0**

Essential controls should be visible without scrolling:

- start/pause/stop appropriate to mode;
- mode;
- tempo;
- loop;
- hand selection;
- metronome;
- count-in;
- reference playback;
- focus/fullscreen;
- exit/back.

Controls should be represented by clear buttons/segmented controls/popovers rather than a long stack of form labels and selects.

Advanced options can live behind one compact settings control.

## UI-23 — MIDI status is visible but quiet

**Decision: High priority**

Show a compact state such as:

- connected device name;
- disconnected/reconnecting;
- permission required.

Use a small status control in the practice chrome. Do not reserve a large callout when everything is healthy.

Escalate to an actionable banner only when the learner must intervene.

## UI-24 — Lesson goal is compact in practice

**Decision: Recommended**

The learner should know the current goal, but a large goal card must not push the score below the fold.

Preferred:

- one concise goal line in the practice header;
- expandable details if needed.

## UI-25 — Mode-specific controls

**Decision: High priority**

Only show controls that matter for the current mode.

Examples:

- Wait: tempo may be secondary;
- Loop: direct score selection + loop control;
- Hands separate: hand selector prominent;
- Sight reading: preparation countdown and restricted retry semantics;
- Performance: minimal chrome and no distracting live judgement unless specifically useful.

## UI-26 — Live feedback must not overload playing

**Decision: High priority**

During active playing:

- use restrained cursor/status feedback;
- avoid a growing list of event chips competing with notation;
- do not flash the entire screen red/green;
- do not force the learner to read detailed text while both hands are occupied.

Detailed diagnosis belongs primarily after the attempt.

## UI-27 — Fingers and technique aids are contextual

**Decision: Recommended**

Fingering should appear in/near the score or as a compact optional aid rather than a large generic list below the score.

Technique video/hand demonstration, when available, should be optional and collapsible.

For a flowkey-like split view, use it only when the visual demonstration materially helps the lesson. Standard notation remains primary.

## UI-28 — Fullscreen/focus mode

**Decision: High priority**

Provide a dedicated focus mode optimized for the music stand/tablet position:

- score dominant;
- minimal control bar;
- no footer;
- no normal navigation;
- no unrelated cards;
- clear way to exit.

---

# 10. Result and next-action screen

## UI-29 — Result answers what happened and what to do next

**Decision: Critical / P0**

Result hierarchy:

1. concise outcome;
2. one primary next action;
3. score/measure problem locations;
4. category details;
5. secondary options.

Examples of primary next action:

- `Practise measures 5–6 at 60 BPM`;
- `Repeat left hand`;
- `Try both hands at 70 BPM`;
- `Continue to next lesson`.

## UI-30 — Category feedback instead of a single grade

**Decision: High priority**

Display only categories supported by the exercise/evidence:

- pitch;
- timing;
- steadiness;
- duration/articulation;
- dynamics;
- pedal.

Each weak category should include:

- what happened;
- where it happened;
- what to do next.

An overall percentage may remain as a compact summary but must not be the main explanation.

## UI-31 — Error locations on score

**Decision: High priority**

Use the score itself as the main diagnostic map.

Error semantics must be distinguishable by more than color, for example:

- icon/marker shape;
- underline/border style;
- short label/tool-tip/popover;
- accessible text list equivalent.

---

# 11. Progress screen

## UI-32 — Skill progress over vanity metrics

**Decision: High priority**

Prioritize:

- note reading;
- sight reading;
- rhythm;
- timing/steadiness;
- ear training;
- technique curriculum;
- chords/accompaniment;
- repertoire.

Secondary information:

- practice time;
- sessions per week;
- completed lessons;
- current review load.

## UI-33 — Simple trends over ambiguous visualizations

**Decision: Recommended**

Prefer:

- trend lines;
- compact bars;
- progress ranges;
- before/after comparisons;
- recent evidence lists.

Avoid radar/spider charts unless user testing shows they are genuinely easier to understand.

Do not add a global leaderboard.

Streaks, if present, remain secondary and non-punitive.

---

# 12. Setup and MIDI onboarding

## UI-34 — Linear first-run MIDI wizard

**Decision: High priority**

Recommended steps:

1. browser/security capability check;
2. explain and request MIDI permission from an explicit user action;
3. choose device;
4. play several notes to confirm input;
5. test sustain, sostenuto and soft pedal when available;
6. confirm setup and continue to first lesson.

Each step should have one clear action and a visible completion state.

## UI-35 — Diagnostics are secondary

**Decision: Recommended**

Detailed event streams, raw controller values and export controls are useful for troubleshooting but should not be part of the normal first-run learner screen.

Provide a clear `Diagnostics` expansion/page when needed.

## UI-36 — Recoverable connection state

**Decision: High priority**

When MIDI disconnects during practice:

- preserve the current practice state;
- show a compact actionable message;
- reconnect automatically where possible;
- allow device reselection without leaving the practice flow.

---

# 13. Responsive behavior

## UI-37 — Design around real piano positions

**Decision: High priority**

Primary score-practice targets:

1. landscape tablet on a music stand;
2. laptop/desktop near the piano;
3. large desktop monitor.

Phone remains fully supported, but it is not the ideal target for dense grand-staff score practice.

## UI-38 — Phone priorities

**Decision: Recommended**

Phone should be particularly strong for:

- Today plan;
- course browsing;
- song browsing;
- ear training;
- rhythm drills;
- progress;
- setup/status;
- short theory/explanation lessons.

For score practice:

- preserve functionality;
- provide zoom/scroll intelligently;
- avoid pretending a dense full-page engraving is comfortably readable on all phones.

## UI-39 — Breakpoints are behavior-based

**Decision: Recommended**

Do not define responsive design only as arbitrary device names.

Layout should change when:

- navigation no longer fits comfortably;
- notation becomes too narrow;
- controls need wrapping;
- split video/score no longer remains legible.

---

# 14. Accessibility and interaction quality

## UI-40 — Touch and pointer targets

**Decision: Required**

Interactive controls should normally provide approximately 44px of practical touch target size where space allows, especially in the practice toolbar and mobile navigation.

## UI-41 — Visible keyboard focus

**Decision: Required / keep**

Keep the existing strong focus principle and extend it to all redesigned custom controls.

No essential function may depend on hover alone.

## UI-42 — Never encode correctness only by color

**Decision: Required**

Correct/current/wrong/missed states require an additional cue such as text, icon, shape, pattern or border treatment.

## UI-43 — Reduced motion

**Decision: Required / keep**

Every animation and transition must respect the user's reduced-motion preference.

## UI-44 — Score readability controls

**Decision: High priority**

Provide:

- useful zoom;
- large score/focus layout;
- sufficient score contrast;
- no decorative background behind notation that harms readability.

## UI-45 — Video and audio control

**Decision: Required**

Instructional video must not autoplay unexpectedly with sound.

Provide captions/text alternatives where required by the lesson content and accessibility policy.

---

# 15. Explicit UI non-goals

Do not implement these as core Nadiano design patterns:

- falling-note/Guitar-Hero lane as the default practice screen;
- constant red/green flashing while playing;
- giant score/XP number as the main learning result;
- global leaderboards;
- loot boxes;
- aggressive streak-loss warnings;
- confetti after routine actions;
- cartoon mascot interrupting practice;
- decorative piano keyboards on every page;
- excessive gradients or glass surfaces;
- a dashboard made almost entirely from equal-weight cards;
- deeply nested tabs and modal flows for basic practice actions;
- tiny low-contrast metadata;
- essential actions available only on hover;
- an always-visible raw MIDI event console;
- duplicating desktop and mobile into separate application architectures.

---

# 16. Implementation priority

## P0 — Redesign foundation

1. define the small design-token set and semantic colors;
2. replace the current peer-link header with the five-destination learner navigation plus secondary profile/settings controls;
3. create the Today screen as the default learner entry;
4. redesign Practice into a notation-dominant focus workspace;
5. redesign Result around problem location + primary next action;
6. preserve accessibility, localization and existing practice behavior while changing presentation.

## P1 — Core product surfaces

7. redesign Learn as a clear curriculum path;
8. redesign Songs/library with search, filters, current pieces and private imports;
9. redesign Train around skill modules and recommendations;
10. redesign Progress around skill evidence and readable trends;
11. redesign setup/MIDI onboarding as a linear wizard;
12. complete landscape tablet, desktop and phone responsive rules;
13. run a focused accessibility pass on all new interactive components.

## P2 — Refinement

14. optional hand/technique split view where content needs it;
15. optional dim/dark practice-focus treatment without a second styling architecture;
16. subtle state transitions and micro-interactions;
17. user testing of score density, control placement and tablet ergonomics;
18. refine visual identity/branding after the structural UI works well.

---

# 17. Definition of done for the UI redesign

A major Nadiano UI redesign is not complete merely because new CSS exists.

It is complete when:

- normal learners see only five primary learning destinations;
- Today provides an obvious start/continue action;
- score-based practice uses most of the useful viewport on landscape tablet/desktop;
- essential practice controls are reachable without vertical scrolling;
- healthy MIDI state is visible but unobtrusive;
- disconnect/reconnect remains recoverable;
- result feedback identifies category, location and next action;
- Learn, Songs, Train and Progress have distinct information purposes;
- imported and bundled pieces use the same visual practice flow;
- German and Indonesian remain equivalent;
- keyboard, pointer and touch operation work on core paths;
- focus states are visible;
- correctness is not communicated by color alone;
- reduced-motion behavior remains valid;
- the UI works at practical phone, landscape tablet and desktop widths;
- no duplicate SPA/design-system architecture is introduced;
- automated accessibility checks remain green and manual keyboard review is completed.

# 18. Backlog maintenance

Before implementing a UI item:

1. confirm which existing 1.0 behavior can be retained unchanged;
2. create the smallest learner-facing vertical slice;
3. specify desktop/tablet/phone behavior for that slice;
4. specify keyboard and reduced-motion behavior;
5. add/update German and Indonesian resources;
6. keep practice/scoring logic deterministic and separate from visual presentation;
7. update this document when a design decision changes.
