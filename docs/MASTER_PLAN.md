# Nadiano master plan

Status: canonical product, learning, UI/UX and implementation plan

Current stable release: **1.0.4**

Last consolidated: **2026-08-11**

This document is the single source of truth for Nadiano's product direction. If another planning document disagrees with this file, this file wins and the conflicting document must be corrected in the same change.

The supporting documents remain useful for detail and evidence:

- `PRODUCT_CONCEPT.md` — product principles and user journeys;
- `LEARNING_CURRICULUM.md` — curriculum detail;
- `CONTENT_MODEL.md` — lesson/package schemas;
- `TECHNICAL_ARCHITECTURE.md` — implementation architecture;
- `QUALITY_AND_RELEASE.md` — quality gates;
- `RESEARCH_BASIS.md` — pedagogical and product research;
- `RELEASE_1_0_CHECKLIST.md` — current stable release evidence and manual gates;
- `BACKLOG.md` — active work only;
- `ROADMAP.md` — version sequence only.

Historical Alpha/Beta checklists and limitation files are evidence for those historical releases and are not descriptions of the current product.

---

# 1. Product mission

Nadiano is a self-hosted browser-based piano learning system for real digital pianos connected through USB MIDI. It is designed first for household use and should remain understandable, maintainable and useful without a cloud dependency.

The goal is not to make a piano game. The goal is to help a learner build transferable musicianship:

- keyboard orientation;
- correct note reading on treble and bass staves;
- pulse and rhythm;
- coordination between both hands;
- practical technique and fingering knowledge;
- ear training and imitation;
- harmony, chords and accompaniment;
- repertoire learning;
- sight reading;
- basic improvisation and creative application;
- deliberate practice and self-evaluation.

Nadiano should combine the strongest ideas from structured methods and modern learning applications without copying one product. The intended product character is:

- as simple to enter as Simply Piano;
- as focused on real pieces as flowkey;
- as guided in short learning steps as Skoove;
- as structured in skill assessment and sight reading as Piano Marvel;
- as deliberate about small practice sections as serious traditional instruction;
- less game-like and less dependent on moving/falling-note graphics than Yousician-style experiences.

---

# 2. Non-negotiable product principles

## 2.1 Standard notation is primary

Real staff notation is the default representation for score-based learning. Temporary aids may exist, but Nadiano must teach skills that transfer to printed music and other contexts.

Do not make falling notes, a piano-roll lane or Guitar-Hero-style targets the default learning mode.

## 2.2 Measure only what can be measured

MIDI can provide objective evidence for:

- pitch;
- note-on timing;
- note-off timing;
- velocity;
- sustain, sostenuto, soft pedal and supported controller events;
- ordering and simultaneity.

MIDI cannot reliably determine:

- posture;
- wrist/arm/shoulder position;
- muscle tension;
- actual finger used;
- acoustic room tone;
- musical intention.

The app must never claim those physical aspects were automatically verified from MIDI. They remain instruction, demonstration and self-assessment unless a future sensor modality is separately researched and validated.

## 2.3 Feedback must lead to an action

A result is not complete when it only displays a percentage.

Useful feedback should answer:

1. What happened?
2. Where did it happen?
3. Which skill is affected?
4. What should the learner do next?

Examples:

- practise measures 5–6 at 60 BPM;
- repeat left hand only;
- clap the rhythm first;
- listen once and copy the phrase;
- repeat with the technique cue "loose wrist";
- continue because current mastery evidence is sufficient.

## 2.4 Skills develop in parallel

A learner must not be able to finish the main path by memorizing repertoire while remaining weak in reading, rhythm or hearing.

The curriculum therefore maintains parallel evidence for reading, rhythm, timing, ear, technique, chords/accompaniment, repertoire and sight reading.

## 2.5 Assistance fades with mastery

Beginner support is temporary. Depending on the exercise, Nadiano may initially show:

- note names;
- keyboard-position hints;
- fingering;
- stronger current-note highlighting;
- wider introductory timing tolerances.

As evidence improves, unnecessary aids should be reduced. The learner must not become dependent on Nadiano-specific overlays.

## 2.6 Content is data

Bundled lessons and private imports use the same validated content/practice path. Do not hard-code a separate runtime for imported music.

MusicXML/MXL is the canonical notation interchange format. MIDI can be reference/performance data but is not automatically treated as a complete notated lesson.

## 2.7 Keep the architecture simple

Nadiano remains a modular monolith:

- ASP.NET Core 10 Razor Pages;
- `Nadiano.Core` for framework-independent learning/scoring logic;
- TypeScript browser modules for MIDI, audio, notation and practice interaction;
- EF Core + SQLite;
- one Docker container and one persistent `/data` volume.

Do not add a SPA framework, microservices, CQRS, MediatR, a generic repository framework, event bus or cloud service without a concrete measured requirement and an architecture decision record.

---

# 3. Current stable baseline — 1.0.4

The current stable product is **1.0.4**. This is the baseline all future work must preserve.

## 3.1 Learning/content baseline

The stable product already contains:

- F0, F1, B1 and B2 progression plus selected introductory E1 material;
- more than the minimum 1.0 guided-lesson requirement;
- at least 120 deterministic rhythm/technique configurations;
- at least 80 reading configurations;
- 60 deterministic ear-training tasks;
- 24 original mini-pieces;
- 12 verified public-domain melodies in independently authored Nadiano study editions;
- stage checks and final beginner assessment;
- review scheduling and skill evidence;
- German and Indonesian content/interface parity gates.

## 3.2 Practice baseline

The current engine supports the stable 1.0 practice set, including where content is compatible:

- Wait;
- Rhythm;
- Loop;
- Hands separate;
- Tempo ladder;
- Listen/copy;
- Performance;
- Sight reading;
- metronome and count-in;
- reference playback;
- section practice;
- deterministic pitch/timing evaluation;
- duration/articulation observations;
- steadiness observations;
- basic dynamics/velocity observations;
- pedal observations.

The score is the primary practice surface. The result view identifies problem locations and provides a next action.

## 3.3 MIDI baseline

Real-device evidence already confirms the complete piano keyboard is recognized.

The application handles and displays the three common piano pedals separately when the device provides them:

- Sustain — CC64;
- Sostenuto — CC66;
- Soft/una-corda — CC67.

The current setup experience is a progressive flow:

1. browser/security capability check;
2. explicit MIDI permission action;
3. device selection;
4. played-key and pedal test;
5. completion/continue.

Detailed raw diagnostics remain secondary.

## 3.4 Current learner information architecture

Normal learner navigation has exactly five primary destinations:

**Today | Learn | Songs | Train | Progress**

German:

**Heute | Lernen | Songs | Trainieren | Fortschritt**

Profile, settings, language, MIDI setup and diagnostics remain secondary controls.

## 3.5 Current UI baseline

1.0.4 already implements the structural UI redesign:

- five-destination learner navigation;
- Today driven by real progress/review/recommendation data;
- distinct Learn, Songs, Train and Progress purposes;
- bundled and private/imported repertoire in one Songs surface;
- Songs text/source/status filtering;
- score-dominant focused Practice workspace;
- compact practice controls;
- quieter live feedback;
- problem-measure + next-action result hierarchy;
- source-aware return from Practice;
- progressive MIDI setup;
- responsive phone navigation;
- preserved keyboard focus, reduced-motion and automated accessibility baseline.

This structural redesign is complete as a stable baseline. Future UI work is refinement and feature completion, not another wholesale navigation rewrite.

## 3.6 Operations baseline

Stable release gates include:

- frontend build, lint, tests and dependency audit;
- .NET format, build and tests;
- content validation;
- browser critical path and accessibility baseline;
- non-root Docker image build;
- Trivy Critical/High policy;
- one CPU / 512 MiB performance profile;
- upgrade rehearsal;
- cold restore rehearsal;
- rollback rehearsal;
- immutable semantic and commit-SHA image tags;
- dependency and third-party license reports.

Production deployment in `Juloc/docker` currently uses `ghcr.io/juloc/nadiano:1.0.4`, container port `8080`, host port `18200`, and persistent `/data`.

---

# 4. Manual acceptance still required

Automation must not be represented as proof for hardware/human-only checks.

The following remain manual acceptance items until explicitly evidenced:

- German first-run to first result in current Chrome with real browser permissions;
- Indonesian first-run to first result in current Edge with real browser permissions;
- PWA installation and MIDI reconnect on the production HTTPS deployment;
- manual keyboard-only review of the core workflows in supported desktop browsers;
- human musical/pedagogical review of bundled repertoire and lesson wording;
- human German/Indonesian localization review;
- licensing/attribution sign-off;
- sustained household/invited-user daily-use evidence without manual database repair.

These items do not block development of 1.x features, but they must remain visible in release documentation and must not be silently checked off by CI.

---

# 5. Target learner experience

## 5.1 First run

The ideal first-run journey is:

1. choose language;
2. create/select learner profile;
3. browser capability explanation;
4. explicitly grant MIDI permission;
5. choose the piano input;
6. play several distinct keys;
7. test sustain, sostenuto and soft pedal when available;
8. choose notation/note-name preference;
9. complete a short orientation/placement flow;
10. receive a recommended first session.

If MIDI is unavailable, the learner may still use reading demonstrations, theory and ear/rhythm content that does not require MIDI.

## 5.2 Daily session

Today should ultimately be a session composer, not only a dashboard.

The recommended default session pattern is:

1. short physical/coordination warm-up;
2. one due review item;
3. one new skill/lesson;
4. current repertoire work;
5. sight-reading or ear/rhythm task;
6. short summary and next recommendation.

The learner should be able to choose approximately **10, 20 or 30 minutes**. The app should scale the number/size of tasks instead of merely truncating text.

Rules:

- one clear `Start/Continue today's practice` action;
- explain why each task was chosen on demand;
- allow skipping without punitive language;
- save/resume an interrupted daily plan;
- do not make streak preservation more important than practice quality;
- use current review queue and skill evidence, not a second recommendation system.

## 5.3 Guided lesson loop

Where appropriate, lessons follow:

**Explain → Demonstrate → Try slowly → Target the difficult part → Play musically → Review → Next action**

Each lesson should have:

- one clear goal;
- why the skill matters;
- concise explanation;
- original/authorized demonstration where helpful;
- one common mistake or contrast where useful;
- a low-complexity first attempt;
- application in a short musical context;
- measurable completion criteria;
- self-check for non-MIDI physical cues;
- later scheduled review.

## 5.4 Deliberate micro-practice

Repeatedly replaying an entire piece is not the preferred response to a localized problem.

When evidence identifies a recurring difficult passage, Nadiano should:

1. select the smallest musically useful section, commonly one or two measures;
2. identify the dominant error category;
3. choose one primary intervention;
4. lower tempo if timing/coordination is unstable;
5. isolate a hand if coordination is the main issue;
6. use rhythm-only practice if rhythm is the main issue;
7. use listen/copy if hearing/phrase memory is the issue;
8. require a small mastery criterion;
9. reinsert the passage into surrounding measures;
10. schedule delayed review.

Do not hard-code a fixed number such as exactly five repetitions for every learner and every problem.

---

# 6. Curriculum and learning domains

Nadiano's curriculum should remain competency-based rather than a single repertoire ladder.

## 6.1 Foundation/keyboard orientation

Teach:

- seating and bench concepts through instruction/self-check;
- relaxed hand shape as guidance, never MIDI-verified;
- groups of black keys and keyboard geography;
- finger numbers;
- high/low, left/right;
- pulse and basic note values;
- simple five-finger patterns;
- first treble/bass anchors.

## 6.2 Note reading

Teach:

- grand staff orientation;
- landmark notes;
- interval reading;
- steps/skips/repeats;
- treble and bass clefs in parallel;
- accidentals;
- key signatures gradually;
- reading by patterns rather than permanent note-name labels;
- increasing range and hand-position changes.

## 6.3 Rhythm

Teach:

- steady pulse;
- quarter/half/whole values and rests;
- eighth-note subdivisions;
- ties;
- dotted values;
- common meters;
- counting strategies;
- syncopation later within supported levels;
- rhythm-only reading and imitation.

## 6.4 Technique

Teach progressively:

- five-finger patterns;
- finger coordination and independence;
- hand-position changes;
- thumb crossing where appropriate;
- scales;
- chords/inversions;
- arpeggios;
- repeated-note control;
- legato/staccato and basic articulation;
- practical pedal coordination.

Technique drills must be short and connected to a musical purpose. Hanon-style material may be used selectively but must not become the central method.

## 6.5 Ear training

Progression should include:

- same/different;
- direction;
- single-note matching;
- interval recognition;
- rhythm imitation;
- short melody imitation;
- major/minor recognition;
- basic chord quality;
- chord progression recognition later.

Ear training should connect hearing to the keyboard, not remain only multiple-choice trivia.

## 6.6 Chords and accompaniment

A dedicated practical path is a high-priority 1.x addition.

Suggested order:

1. major/minor triads;
2. chord symbols;
3. root-position recognition;
4. inversions;
5. left-hand chord patterns;
6. broken chords;
7. reusable accompaniment patterns;
8. lead-sheet reading;
9. simple pop accompaniment;
10. common progressions;
11. melody + chord-symbol combination;
12. basic improvisation.

This path must reuse shared theory/ear/rhythm skills and must not duplicate the entire curriculum.

## 6.7 Sight reading

Sight reading is a separate skill and should have separate evidence.

Rules:

- unfamiliar material only for a fresh sight-reading score;
- short preview period before the attempt;
- no rewind during the assessment attempt;
- first attempt determines the sight-reading evidence;
- subsequent repeats may become ordinary practice but not another fresh sight-reading score;
- difficulty should adapt by note range, rhythm, intervals, accidentals, chord density, hand changes, hands together and tempo.

## 6.8 Repertoire

Repertoire should develop musical application rather than merely generate scores.

Each piece may contribute evidence to relevant skills, but a high repertoire result must not automatically clear unrelated technique/reading skills.

Bundled repertoire should remain original, appropriately licensed or verified public domain with Nadiano's own study edition/engraving and attribution.

## 6.9 Improvisation/creative work

Introduce after basic rhythm, scales/chords and ear foundations.

Early activities may include:

- limited-note call and response;
- improvise over a simple chord loop;
- vary a rhythm;
- create a short ending;
- choose chord tones;
- simple left-hand accompaniment with invented melody.

Technology should support the activity without pretending to fully judge musical creativity automatically.

---

# 7. Practice modes

The target stable practice model includes:

## Explore

Free playing with live keyboard/status information. Useful for orientation and diagnostics, not a scored lesson by default.

## Wait

The score waits for the required pitch/chord. Good for early note learning and low-pressure decoding.

## Rhythm

Pitch may be simplified or fixed while onset, pulse and duration are emphasized.

## Hands separate

Practice a selected hand/voice before recombination.

## Loop

Repeat a selected range with optional count-in and adaptive tempo.

## Tempo ladder

Increase tempo after stable successful evidence. Steps are configurable/adaptive; fixed percentages are examples, not rules.

## Listen and copy

Hear a short phrase and reproduce it. Useful for ear, phrasing and memory.

## Performance

Play through with minimal interruption. Detailed diagnosis occurs after the attempt.

## Sight reading

Preview once, attempt once as unseen material, then move it to ordinary practice if desired.

## Free practice/recording

Later 1.x option for self-review without predefined expected notes. MIDI performance recording can precede optional audio recording.

---

# 8. Scoring and feedback model

## 8.1 Pitch

Identify:

- correct notes;
- missed notes;
- extra notes;
- wrong pitches;
- chord completeness;
- supported repeated-note behavior.

## 8.2 Timing

Provide learner-readable categories such as early/late and show location.

## 8.3 Steadiness

Measure consistency separately from one isolated onset error.

## 8.4 Duration/articulation

Where evidence supports it, distinguish too short/too long and basic articulation expectations.

## 8.5 Dynamics

Use MIDI velocity only as relative evidence. Do not call it a complete measurement of tone quality.

## 8.6 Pedal

Evaluate only content that declares a pedal expectation and only controllers the device reports.

Keep CC64/66/67 separate.

## 8.7 Result hierarchy

1. concise outcome;
2. one primary next action;
3. score/measure problem locations;
4. category detail;
5. secondary options.

Correct/wrong/current states must never rely on color alone.

---

# 9. Adaptive learning and review

The recommendation engine should remain explicit and testable.

Inputs may include:

- recent skill evidence;
- review-due items;
- repeated error category;
- repeated error location;
- current lesson prerequisites;
- current repertoire state;
- current target tempo;
- prior hands-separate performance;
- learner-selected daily duration.

Rules should be explainable, for example:

- timing weak in the same measures twice → smaller loop + slower tempo;
- both-hands score lower than each hand alone → coordination intervention;
- pitch stable but rhythm weak → rhythm-only intervention;
- item mastered immediately → schedule delayed review rather than permanent completion;
- repeated stable review → increase interval;
- new reading material too easy across several attempts → raise sight-reading difficulty;
- new reading material repeatedly overwhelming → lower one difficulty dimension.

Do not use opaque AI to decide core progression when deterministic rules can do the job.

---

# 10. Main information architecture

Normal learner destinations are fixed unless user testing provides strong evidence otherwise:

## Today

Question answered: **What should I practise now?**

Target content:

- one primary start/continue action;
- current recommended lesson;
- due reviews;
- current piece;
- planned session outline;
- selected 10/20/30 minute duration;
- compact MIDI readiness;
- optional explanation of why each task was chosen.

Do not lead with analytics, streaks or many equal KPI cards.

## Learn

Question answered: **What am I learning and what comes next?**

Use a readable curriculum path grouped into units/stages.

Show:

- current position;
- lesson/unit goal;
- progress/mastery state;
- prerequisites only where relevant;
- review-needed state;
- optional goal emphasis such as notation/solo vs chords/accompaniment.

Do not turn it into a decorative fantasy map.

## Songs

Question answered: **What can I learn or play?**

One library contains bundled and private/imported music.

Target filters:

- search text;
- source;
- readiness/progress;
- difficulty;
- skill;
- genre/style;
- expected learning time;
- hand complexity;
- original/public-domain/private;
- favorites.

A song item should prioritize title, composer/source, difficulty, relevant skills, progress and estimated duration. Do not invent unrelated album art merely to fill cards.

## Train

Question answered: **Which skill do I want to sharpen?**

Top modules:

- Note reading;
- Sight reading;
- Rhythm;
- Ear training;
- Scales & chords;
- Technique;
- Pedal where measurable.

Each module should show current state, one recommended drill and due review when applicable rather than a wall of tiny exercises.

## Progress

Question answered: **What is improving and what needs work?**

Prioritize skill evidence:

- note reading;
- sight reading;
- rhythm;
- timing/steadiness;
- ear training;
- technique curriculum;
- chords/accompaniment;
- repertoire.

Secondary metrics:

- practice time;
- sessions/week;
- completed lessons;
- review load.

Prefer simple trends and recent evidence to ambiguous radar charts.

---

# 11. Practice workspace UI specification

Practice is the most important interaction surface.

## 11.1 Layout

On landscape tablet/laptop/desktop:

- notation receives most useful width and height;
- focused practice is not constrained by the normal narrow content width;
- normal global navigation is hidden;
- unrelated cards/footer are removed;
- score zoom and scrolling remain practical from a music-stand distance.

## 11.2 Persistent controls

Essential controls stay reachable without vertical scrolling:

- start/pause/stop as appropriate;
- current mode;
- tempo;
- loop;
- hand selection;
- metronome;
- count-in;
- reference playback;
- score zoom;
- focus/fullscreen;
- exit/back;
- compact MIDI status.

Advanced or mode-specific settings belong behind a compact secondary control.

## 11.3 Mode-specific disclosure

Only show what matters.

Examples:

- Wait: note progression and help state are more relevant than performance analytics;
- Loop: score range selection is prominent;
- Hands separate: hand selector is prominent;
- Sight reading: preparation timer and assessment restrictions are explicit;
- Performance: chrome is minimal and live judgement is restrained.

## 11.4 Live feedback

Do not make the learner read detailed text while both hands are occupied.

During playing:

- use a quiet cursor/current state;
- use restrained error/current markers;
- avoid growing event-chip lists;
- avoid whole-screen red/green flashes;
- preserve score readability.

Detailed diagnosis comes after the attempt.

## 11.5 Technique/fingering aids

Fingering belongs in/near the score where possible. Technique demonstrations are optional/collapsible and shown only when they materially help.

A split view with hands/video is allowed for specific lessons, not as mandatory layout for every piece.

---

# 12. Visual design system

Nadiano should feel like a calm, modern, high-quality music learning workspace for adults and younger learners without becoming childish.

## 12.1 Style qualities

- clean;
- calm;
- precise;
- friendly but not cartoonish;
- notation-first;
- restrained color;
- strong visual hierarchy;
- moderate whitespace;
- touch-safe controls.

Avoid:

- mascots as the core identity;
- confetti-heavy success;
- neon piano decoration;
- excessive gradients;
- glassmorphism for ordinary controls;
- equal-weight card soup;
- tiny low-contrast metadata.

## 12.2 Color

Use semantic CSS tokens, not page-specific colors:

- canvas;
- surface;
- subtle surface;
- primary text;
- secondary text;
- border/divider;
- accent;
- focus;
- success;
- warning;
- error;
- current notation state.

Rules:

- accent is not automatically success;
- saturated color is reserved for action/status/feedback;
- correctness never depends on red/green alone;
- all final token values must pass contrast checks.

## 12.3 Light/dark

Light is the default because sheet music naturally fits a light paper surface.

A dim/dark focused-practice treatment is optional later only if it does not create a second parallel design architecture and score contrast remains excellent.

## 12.4 Typography

Use one highly legible system/Segoe-style sans-serif stack for UI. Use size, weight and spacing for hierarchy rather than multiple font families.

Hierarchy:

- page title;
- section title;
- module title;
- body;
- supporting/meta text;
- compact labels.

Notation remains visually dominant in Practice.

## 12.5 Spacing

Use a small consistent scale, for example:

- 4;
- 8;
- 12;
- 16;
- 24;
- 32;
- 48 px.

Normal pages are moderately spacious. Practice is denser but controls remain touch-safe.

## 12.6 Surfaces

Prefer spacing/grouping before adding cards.

Use cards only for genuinely separate modules. Avoid deeply nested card-within-card layouts.

## 12.7 Icons

Use one consistent SVG icon family. Icons support labels rather than replacing unfamiliar terms. Do not use emoji as permanent navigation icons.

## 12.8 Motion

Motion only clarifies state changes:

- navigation/content transitions;
- selection/loop changes;
- compact success acknowledgement;
- panel expansion;
- connection-state change.

No continuous decorative animation. Respect `prefers-reduced-motion` everywhere.

---

# 13. Responsive and accessibility rules

## 13.1 Primary device positions

Design score practice first for:

1. landscape tablet on a music stand;
2. laptop/desktop next to the piano;
3. large desktop monitor.

Phone is supported but is not the ideal dense grand-staff practice device.

## 13.2 Phone strengths

Phone should remain strong for:

- Today;
- Learn browsing;
- Songs browsing;
- ear training;
- rhythm drills;
- progress;
- setup/status;
- short explanation/theory lessons.

Score practice remains functional with intelligent zoom/scroll but should not pretend a dense page is comfortable on every phone.

## 13.3 Breakpoints

Use behavior-based breakpoints when navigation, score width or controls no longer fit; do not design separate app architectures for device classes.

## 13.4 Accessibility

Required:

- practical touch targets around 44px where possible;
- visible keyboard focus;
- no required hover-only action;
- semantic headings/landmarks/labels;
- non-color correctness cues;
- reduced-motion support;
- score zoom/scaling;
- text alternatives for technique media;
- no unexpected autoplay with sound;
- captions/text alternatives where lesson media requires them;
- keyboard operation independent of the MIDI keyboard.

---

# 14. Songs, imports and private content

## 14.1 Import

Keep secure private MusicXML/MXL import with:

- file count/size limits;
- XML entity protection;
- archive traversal/expansion protection;
- safe generated storage names;
- preview and warnings;
- hand/voice mapping;
- section definition;
- target tempo;
- fingering metadata/overlays;
- private package publication.

## 14.2 Same runtime path

Bundled and private music must use the same practice/scoring engine.

## 14.3 Imported-piece practice intelligence — planned

A high-priority 1.x feature is generating useful practice suggestions from imported pieces:

- difficult-measure loops;
- hands-separate tasks;
- tempo ladder;
- rhythm-only drill;
- first-use sight-reading classification where appropriate;
- later review scheduling.

This must be based on the same explicit rules used by bundled content.

## 14.4 Notation editing boundary

Do not build a full professional score editor in 1.x.

Allow only practice-oriented metadata/overlays and use MusicXML as the interchange format. Rich notation editing is a 2.0 candidate only if a real need emerges.

---

# 15. Motivation and gamification policy

Recommended:

- flexible daily goal;
- restrained completion acknowledgement;
- meaningful personal challenges;
- optional non-punitive streak;
- optional achievement for real musical milestones.

Secondary/low priority:

- generic XP/player level.

Do not build:

- loot boxes;
- global leaderboards;
- aggressive streak-loss warnings;
- routine confetti sequences;
- competition as the main learning incentive.

Skill evidence and musical progress remain more important than generic points.

---

# 16. Coach and AI policy

The core coach remains deterministic and explainable.

Required behavior:

- one primary next action;
- optional secondary alternatives;
- explain why the recommendation was chosen;
- use real skill/error evidence;
- never diagnose injury/tension.

A generic AI chat must not become the core piano teacher or scoring authority.

Optional later AI use may include:

- alternative explanation wording;
- summarizing learner history;
- answering questions around already validated lesson content.

Any AI layer must not silently override reviewed curriculum, deterministic scoring or privacy rules.

---

# 17. Audio, video and self-review

## 17.1 Technique demonstrations

Recommended 1.x refinement:

- original hand/fingering demonstrations;
- useful camera angle;
- slow motion where helpful;
- text alternative;
- common-correct/incorrect contrast when pedagogically useful.

## 17.2 Backing tracks

Use original/appropriately licensed accompaniment where it helps timing and musical context. It is not required for basic practice.

## 17.3 Local audio recording — later 1.x

Potentially useful for musical self-review beyond MIDI.

Requirements:

- explicit opt-in;
- local by default;
- clear deletion;
- no hidden cloud upload;
- listen-back workflow;
- clear distinction between MIDI metrics and subjective sound review.

## 17.4 Camera-assisted self-review — 2.0 research candidate

Requires separate privacy, validity and UX research. Do not implement posture pass/fail claims merely because camera access exists.

---

# 18. Profiles, teachers and social scope

## Current

Multiple independent local learner profiles remain required.

Each profile owns independent:

- language/notation preferences;
- MIDI preference;
- course progress;
- skill evidence;
- practice history;
- review queue;
- accessibility settings;
- private imported material.

## Later

Teacher features may include:

- private teacher notes;
- assigned practice;
- progress view with explicit permissions.

These are later than the household self-learning improvements.

Do not build live video conferencing as a core feature. Existing conferencing tools solve a different problem.

Do not prioritize multiplayer/group-performance synchronization.

---

# 19. Technical architecture plan

## 19.1 Runtime flow

```text
Digital piano
  -> USB MIDI
  -> learner browser
     - Web MIDI adapter
     - event normalization
     - practice clock
     - scorer
     - notation interaction
     - IndexedDB active-session resilience
  -> HTTPS
  -> ASP.NET Core container
     - Razor Pages
     - content/course services
     - profile/progress services
     - import validation
     - SQLite
  -> /data persistent volume
```

The Docker host never directly accesses the USB piano.

## 19.2 Browser responsibilities

Keep latency-sensitive work local:

- Web MIDI;
- MIDI normalization;
- Web Audio metronome/reference scheduling;
- notation rendering interaction;
- live matching/session state;
- focused practice UI;
- active-session recovery buffer.

## 19.3 Server responsibilities

- profile persistence;
- course/content loading;
- lesson/practice definitions;
- progress and review scheduling;
- import staging/validation/publication;
- version diagnostics;
- export/delete;
- health/logging.

## 19.4 Persistence

Keep explicit relational state and committed EF Core SQLite migrations.

Do not move structured domain state into a generic key-value store.

Private notation/media files live safely under `/data` with generated identifiers and are not executable/served directly from upload paths.

## 19.5 PWA/offline

Core principles:

- installable shell;
- controlled versioned caching;
- prepared lesson assets may work through interruption;
- completed results queued idempotently;
- private imported files not placed in shared caches;
- version-based invalidation;
- visible online/offline state;
- clear offline-data deletion.

Do not promise fully offline import/account management unless explicitly implemented and tested.

---

# 20. Security and privacy

Required baseline:

- external HTTPS;
- browser permission only from explicit learner action;
- restrictive CSP/security headers;
- no runtime CDN requirement;
- XML external entities disabled;
- archive traversal and decompression limits;
- file count/size limits;
- non-root container;
- safe filenames/storage IDs;
- no arbitrary lesson HTML;
- dependency/container scanning;
- no raw private lesson/import data in normal logs;
- raw MIDI not retained longer than needed by the configured evidence/privacy model;
- explicit export/delete for profile data;
- audio/video recording opt-in only if added later.

No external cloud service may become mandatory for the core learning loop.

---

# 21. Testing strategy

Every feature gets the smallest relevant set of tests.

## Unit

- MIDI normalization;
- matching;
- chord/repeated-note behavior;
- timing categories;
- pedal state;
- adaptive rules;
- review scheduling;
- content validation.

## Fixtures

Maintain deterministic MIDI fixtures for clean/incorrect scales, repeated notes, early/late events, chords, pedal overlap, disconnects and malformed/out-of-order events.

## Integration

- migrations/SQLite constraints;
- package loading;
- import security;
- localization handlers;
- idempotent result completion;
- profile separation;
- export/delete.

## Browser

Use the fake MIDI adapter behind the same interface as real Web MIDI. Cover the critical learner path and accessibility structure.

## Manual

Real USB MIDI and real browser permission flows remain manual release evidence. CI must not simulate them and then claim they passed as real hardware testing.

---

# 22. Release/version policy

## 1.0.x

Patch releases are for:

- bug fixes;
- compatibility fixes;
- accessibility corrections;
- security fixes;
- contained UX refinements that do not redefine the learning model.

Current stable: **1.0.4**.

## 1.1 — Daily learning and adaptive practice

Primary goal: turn the stable surfaces into a stronger personalized daily teacher-like practice loop without adding opaque AI.

Planned scope:

1. full 10/20/30-minute Today session composer;
2. resumable daily session state;
3. adaptive one/two-measure mastery flow;
4. explicit error-category interventions;
5. progressive removal of note-name/keyboard/fingering aids;
6. richer skill-specific progress;
7. separate adaptive sight-reading level;
8. stronger explanation of recommendation reasons;
9. advanced Songs filters/favorites/recommended-current sections;
10. user testing of practice density and tablet ergonomics.

Acceptance gate:

- a learner can start a useful daily session with one action;
- the session mixes review/new/repertoire/sight-or-ear work according to evidence;
- repeated localized errors produce targeted micro-practice instead of full-piece retry only;
- aids demonstrably reduce when mastery evidence permits;
- sight-reading difficulty changes through tested explicit rules;
- German/Indonesian parity, accessibility, browser and stable release gates remain green.

## 1.2 — Chords, accompaniment and imported-piece intelligence

Primary goal: make Nadiano equally useful for practical accompaniment and for learner-owned repertoire.

Planned scope:

1. dedicated chord/accompaniment path;
2. triads, inversions and accompaniment patterns;
3. lead-sheet reading;
4. simple pop accompaniment progression;
5. chord/ear integration;
6. initial improvisation exercises;
7. automatic targeted practice suggestions for imported MusicXML/MXL;
8. stronger repertoire metadata/filters;
9. optional original backing tracks where pedagogically useful.

Acceptance gate:

- chord path reuses shared skills rather than duplicating the whole curriculum;
- lead-sheet tasks have objective, testable completion where possible;
- imported and bundled pieces still share the same runtime;
- generated imported-piece drills are deterministic/explainable;
- no copyrighted modern content is bundled without explicit rights.

## 1.3 — Intermediate expansion and richer self-review

Primary goal: extend beyond beginner foundations without weakening quality.

Potential scope:

- expanded E1/intermediate content;
- improved pedal/dynamics analysis;
- richer hand/technique demonstrations;
- printable/large-score layouts;
- more notation naming options/languages;
- optional local audio recording/listen-back;
- teacher notes on private profiles if permissions remain simple.

Every item requires its own vertical-slice acceptance criteria before implementation.

## 2.0 candidates

Not promises. These require new architecture/privacy/licensing decisions:

- assisted MIDI-to-MusicXML workflow;
- teacher/learner account model;
- synchronized multi-device profiles;
- optional camera-assisted self-review;
- richer notation editing;
- licensed/community content distribution.

---

# 23. Explicitly rejected or deferred features

## Do not build as core patterns

- falling-note/Guitar-Hero lane as default practice;
- posture/tension/finger correctness from MIDI;
- global leaderboard;
- loot boxes/random reward mechanics;
- aggressive streak punishment;
- generic AI chat as the scoring/progression authority;
- mandatory cloud service;
- live video conferencing;
- online multiplayer/group synchronization;
- full professional notation editor in 1.x;
- microservices/CQRS/MediatR/event-bus architecture without demonstrated need;
- separate native mobile application while the responsive PWA meets the requirement.

## Optional later only

- generic XP;
- achievements;
- dark/dim practice mode;
- microphone note detection for acoustic pianos;
- local audio recording;
- teacher features;
- camera-assisted self-review.

---

# 24. Current active backlog after 1.0.4

Priority order:

## P0 — 1.1 core

1. Today session composer with 10/20/30-minute plans.
2. Resumable daily session flow.
3. Adaptive micro-practice/mastery loop.
4. Progressive removal of learning aids.
5. Skill-specific progress model refinement.
6. Adaptive sight-reading difficulty/evidence.
7. Recommendation explanation refinement.

## P1 — 1.1/1.2 product depth

8. Songs favorites and richer filters/recommendations.
9. Dedicated chord/accompaniment path.
10. Lead-sheet exercises.
11. Imported-piece targeted practice generation.
12. Original backing tracks where useful.
13. Richer technique demonstrations.
14. Weekly/error-trend progress views where enough evidence exists.

## P2 — later refinement

15. personal challenges;
16. optional non-punitive streak/achievements;
17. optional dim practice focus theme;
18. printable/large-score layouts;
19. local audio recording/listen-back;
20. teacher notes/assignments after household workflow is mature.

## Manual acceptance track

In parallel, complete the manual gates listed in section 4 and record evidence in `RELEASE_1_0_CHECKLIST.md` or the corresponding future release checklist.

---

# 25. Work-package plan from the current baseline

Historical WP-001 through WP-045 delivered the foundation through stable 1.0. The active work package sequence starts here.

## WP-046 — Documentation consolidation

Goal: one canonical plan, no contradictory current-status documents.

Done when:

- `MASTER_PLAN.md` is canonical;
- README, Roadmap, Backlog, UI backlog and Handover point to it;
- historical Alpha/Beta docs are clearly historical;
- current stable version references are 1.0.4.

## WP-047 — Today session composer

Deliver:

- 10/20/30-minute plan selection;
- real task composition from review queue, recommendation, repertoire and sight/ear work;
- one start/continue action;
- reason per task;
- resumable state.

Tests:

- deterministic composition for fixed evidence/time budget;
- no unrelated-skill clearing;
- localization/browser/accessibility coverage.

## WP-048 — Adaptive micro-practice

Deliver:

- recurring problem-section detection;
- one/two-measure targeted loops;
- tempo/hands/rhythm/listen interventions;
- mastery threshold;
- reintegration into surrounding passage;
- delayed review.

Tests:

- deterministic rules;
- repeated fixtures;
- no endless loop when evidence is insufficient;
- explanation matches chosen rule.

## WP-049 — Assistance fading

Deliver:

- explicit help levels;
- lesson/content policy for eligible aids;
- evidence-based fade rules;
- learner override/accessibility escape hatch.

Tests:

- aids reduce only when configured evidence is sufficient;
- accessibility preference can retain needed aid;
- no silent change mid-attempt.

## WP-050 — Skill progress and adaptive sight reading

Deliver:

- separate sight-reading evidence;
- difficulty dimensions;
- tested up/down adjustment;
- Progress presentation;
- fresh/unseen attempt semantics.

## WP-051 — Chord/accompaniment path

Deliver the first complete vertical slice from triads through one reusable accompaniment pattern and a lead-sheet task before expanding the catalogue.

## WP-052 — Imported-piece practice intelligence

Deliver deterministic practice-task suggestions from imported MusicXML/MXL without adding a second practice engine.

## WP-053 — Repertoire/technique refinement

Deliver richer Songs metadata, favorites/recommendations, useful original demonstrations and backing tracks where content justifies them.

## WP-054 — 1.1 release hardening

Run the full release pipeline plus manual focused UX checks for the changed daily/adaptive flows. Publish only after migration/backup compatibility and rollback are verified.

---

# 26. Definition of done for any future feature

A feature is complete only when all relevant items are satisfied:

- learner outcome is explicit;
- scope and exclusions are explicit;
- architecture remains simple;
- behavior is implemented, not only visually mocked;
- deterministic rules are tested;
- German and Indonesian are equivalent;
- error/recovery states exist;
- accessibility is maintained;
- phone/tablet/desktop behavior is defined where relevant;
- persistence/migration impact is handled;
- backup/restore compatibility is considered;
- documentation is updated;
- no release/manual gate is falsely claimed as automated evidence.

For UI changes additionally:

- exactly one primary action per important state where reasonable;
- no essential control is hover-only;
- score remains primary during score practice;
- touch targets remain practical;
- correctness is not color-only;
- reduced motion is honored;
- existing MIDI/scoring behavior is not duplicated in a visual layer.

---

# 27. Documentation authority and maintenance rule

To prevent the repository from drifting into contradictory plans again:

1. **`MASTER_PLAN.md` is authoritative for scope, priorities and current/future status.**
2. `ROADMAP.md` must contain only version sequencing and milestone gates.
3. `BACKLOG.md` must contain only active unfinished work and deliberate non-goals.
4. `UI_UX_BACKLOG.md` must contain only remaining UI refinements/research notes, not an obsolete "current UI" description.
5. `JUNIOR_IMPLEMENTATION_PLAN.md` must contain only active work packages from the current baseline; completed historical packages are summarized, not presented as future work.
6. `HANDOVER.md` must describe the current stable state, not Alpha-era incomplete work.
7. Release checklists contain release evidence; they do not redefine product scope.
8. Historical Alpha/Beta files keep their version in the title and are not edited to pretend they describe stable releases.
9. Any plan change that alters scope must update this master plan first or in the same commit.
10. If code behavior and this plan disagree, either fix the code or explicitly update the plan; never leave both as competing truths.
