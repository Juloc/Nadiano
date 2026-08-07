# Nadiano product backlog

This backlog records product and learning improvements identified after the 1.0 release and a comparison with current piano-learning products and established piano-teaching approaches.

It is intentionally product-oriented. Items describe the learner outcome first and should later be split into small vertical implementation slices with acceptance criteria before coding.

## Decision legend

| Decision | Meaning |
|---|---|
| **Recommended** | Should become part of Nadiano unless implementation research finds a concrete blocker. |
| **High priority** | Recommended and expected to improve the core learning loop significantly. |
| **Optional** | Useful, but not required for the core learning experience. |
| **Later** | Useful after the core post-1.0 learning experience is improved. |
| **Do not build** | Deliberately excluded because it adds complexity, teaches the wrong behavior or cannot be evaluated honestly. |

## Current product direction

The strongest direction for Nadiano is not to copy one competitor. It should combine:

- a very simple daily entry point;
- a structured course from beginner to advanced material;
- strong score-based practice for real pieces;
- short targeted drills for reading, rhythm, technique and ear training;
- adaptive recommendations based on actual evidence;
- honest MIDI feedback without pretending to detect posture or physical tension;
- real standard notation as the main visual language;
- optional assistance that is gradually removed as the learner improves.

The preferred top-level learner structure is:

**Today | Learn | Songs | Train | Progress**

A separate UI/visual-design review is still required. The final visual style, component hierarchy, spacing, motion, typography, colors, practice-screen layout and responsive rules are **not yet considered finished by this backlog**.

---

# P0 — Core learning-flow improvements

## 1. Today — automatic daily practice plan

**Decision: High priority**

Create a clear home entry point that builds a useful session automatically instead of forcing the learner to choose individual exercises.

Recommended session pattern:

1. short warm-up;
2. due review item;
3. one new skill;
4. work on a piece;
5. short sight-reading or ear-training task;
6. clear session summary and next recommendation.

Recommended options:

- 10, 20 and 30 minute session lengths;
- explain why each item was selected;
- adjust the plan from skill evidence and recent errors;
- allow skipping without punishing the learner;
- resume an interrupted session.

**Why:** reduces decision fatigue and turns Nadiano into a daily learning system rather than a collection of exercises.

## 2. Consistent lesson loop

**Decision: High priority**

All guided lessons should follow a predictable teaching sequence where appropriate:

**Explain → Demonstrate → Try slowly → Target the difficult part → Play musically → Review result → Recommend next action**

Requirements:

- lesson starts with one clear objective;
- demonstration can be heard and, where useful, visually shown;
- first attempt may use stronger assistance;
- assistance is reduced progressively;
- results identify the specific skill and location of errors;
- the learner always gets a concrete next action.

**Why:** a consistent learning loop is easier to understand and supports deliberate practice.

## 3. Adaptive micro-practice for difficult measures

**Decision: High priority**

When a learner repeatedly fails a passage, Nadiano should create a focused practice sequence instead of simply asking for another full attempt.

Recommended behavior:

- identify the smallest useful problem section, commonly one or two measures;
- reduce tempo when timing is unstable;
- recommend hands separately when coordination is the main problem;
- use rhythm-only practice when rhythm is the main problem;
- repeat until the section reaches a clear mastery threshold;
- reinsert the section into surrounding measures before considering it learned;
- schedule a later review instead of assuming immediate mastery is permanent.

**Why:** this mirrors effective human practice better than repeatedly replaying an entire piece.

## 4. Progressive removal of assistance

**Decision: High priority**

Beginner assistance should be temporary rather than permanent.

Possible assistance levels:

- note names shown;
- keyboard position highlighted;
- fingering shown;
- stronger current-note highlighting;
- larger timing tolerance in introductory exercises.

As evidence improves, Nadiano should gradually remove unnecessary help.

**Why:** prevents dependency on labels and highlights and develops real notation reading.

---

# P0 — Main navigation and product structure

## 5. Today

**Decision: High priority**

Primary daily practice entry point. See the automatic daily practice plan above.

## 6. Learn

**Decision: Recommended**

Structured curriculum and course map.

Should contain:

- foundations;
- note reading;
- rhythm;
- technique;
- ear training;
- chords and accompaniment;
- repertoire skills;
- introductory improvisation when prerequisites are met.

The course may have prerequisites, but it should not lock every unrelated activity behind a single strict linear path.

## 7. Songs

**Decision: Recommended**

Dedicated repertoire library for bundled and imported pieces.

Should support:

- difficulty filters;
- skill filters;
- favorites;
- current pieces;
- completed pieces;
- public-domain and original content attribution;
- private MusicXML/MXL pieces.

## 8. Train

**Decision: Recommended**

Short targeted practice independent of the main course.

Categories:

- sight reading;
- note reading;
- rhythm;
- ear training;
- scales;
- chords;
- arpeggios;
- finger coordination;
- pedal control where measurable;
- technique self-review prompts.

## 9. Progress

**Decision: Recommended**

Show evidence by skill, not only a single overall number.

Recommended views:

- note-reading level;
- rhythm level;
- timing steadiness;
- ear-training level;
- technique curriculum progress;
- chord/accompaniment progress;
- repertoire progress;
- sight-reading level;
- recent practice history;
- recurring weak areas;
- review queue.

---

# P1 — Repertoire and song-learning experience

## 10. Strong dedicated song practice screen

**Decision: High priority**

The piece-learning experience should be one of Nadiano's strongest screens.

Recommended controls:

- standard notation as the primary view;
- current position/cursor;
- play reference;
- wait mode;
- loop selected measures;
- left hand only;
- right hand only;
- both hands;
- tempo control;
- tempo ladder;
- count-in;
- metronome;
- performance mode;
- sight-reading mode where appropriate;
- section selection directly from the score;
- clear error locations after the attempt.

## 11. Hands/fingering demonstration

**Decision: Recommended**

Where useful and legally possible, lessons may include an original visual demonstration of hand movement or fingering in addition to notation.

Requirements:

- demonstration is an aid, not a replacement for notation;
- text alternative for accessibility;
- no claim that MIDI detected the physical finger actually used.

## 12. Tempo ladder

**Decision: Recommended**

Allow structured progression such as 50% → 60% → 70% → 80% → 90% → target tempo.

The actual steps should adapt to performance rather than always using fixed percentages.

**Why:** promotes stable movement before full-speed performance.

## 13. Backing/accompaniment tracks

**Decision: Recommended where content supports it**

Use original or appropriately licensed accompaniment to develop timing and musical context.

Do not make accompaniment required for basic practice.

## 14. Auto-scroll / score following

**Decision: Recommended**

The learner should not need to touch the screen while playing longer pieces.

---

# P1 — Reading and sight reading

## 15. Standard notation remains primary

**Decision: High priority**

Real staff notation must remain the default visual representation for learning pieces and reading exercises.

**Why:** Nadiano should teach transferable piano reading, not only interaction with Nadiano.

## 16. Optional note-name assistance

**Decision: Recommended**

Allow note names for beginners, then reduce or remove them as reading improves.

## 17. Optional keyboard-position assistance

**Decision: Recommended, temporary**

Useful for orientation at the beginning, but should fade out as soon as the learner can locate notes independently.

## 18. Adaptive sight-reading level

**Decision: High priority**

Maintain a separate sight-reading skill level and generate/select unfamiliar material at an appropriate difficulty.

Should consider:

- note range;
- hand position changes;
- rhythm complexity;
- accidentals;
- interval size;
- chord density;
- hands together/separate;
- tempo;
- articulation complexity.

## 19. Preparation time before sight reading

**Decision: Recommended**

Give a short score-inspection period before the first attempt.

## 20. Do not immediately repeat a sight-reading test as another sight-reading score

**Decision: Recommended rule**

After the first attempt the material is no longer unseen. It may become ordinary practice, but should not be counted as a fresh sight-reading result.

## 21. Falling-note / Guitar-Hero view

**Decision: Do not use as the primary learning mode**

A temporary optional visualization could be considered for absolute beginners or demonstrations, but it must not replace standard notation or become the default practice experience.

**Why:** following falling targets can train screen reaction without developing transferable score reading.

---

# P1 — MIDI feedback and practice analytics

## 22. Pitch correctness

**Decision: Required / keep**

Identify correct, missed and extra notes deterministically.

## 23. Early/late timing feedback

**Decision: Required / keep**

Show timing errors in a learner-readable form.

## 24. Duration and articulation feedback

**Decision: Recommended**

Where MIDI evidence supports it, distinguish notes held too briefly/too long and basic articulation categories.

## 25. Steadiness feedback

**Decision: Recommended**

Measure rhythmic consistency separately from individual note timing.

## 26. Dynamics / velocity feedback

**Decision: Recommended with limitations**

Use MIDI velocity for useful relative dynamic feedback where the keyboard supports meaningful velocity values.

Do not treat velocity as a complete measurement of musical tone production.

## 27. Pedal feedback

**Decision: Recommended / keep improving**

Support separate diagnostic and learning feedback for:

- sustain (CC64);
- sostenuto (CC66);
- soft pedal (CC67).

## 28. Error markers directly in the score

**Decision: High priority**

After a performance, visually identify where pitch, timing, duration or pedal issues occurred.

## 29. Category scores

**Decision: Recommended**

If a numeric score is shown, split it into useful categories such as:

- pitch;
- timing;
- steadiness;
- duration/articulation;
- dynamics/pedal where supported.

## 30. Single unexplained overall score

**Decision: Do not use alone**

An overall score may exist as a summary, but must never replace specific feedback and next actions.

---

# P1 — Ear training

## 31. Single-note recognition

**Decision: Recommended**

## 32. Interval recognition

**Decision: Recommended**

## 33. Major/minor and chord-quality recognition

**Decision: Recommended**

## 34. Rhythm imitation

**Decision: Recommended**

## 35. Melody imitation by ear

**Decision: High value**

Hear a short melody and reproduce it on the piano.

**Why:** directly connects hearing with keyboard control.

## 36. Chord progression recognition

**Decision: Later**

Useful after basic intervals and chord qualities are established.

---

# P1 — Rhythm training

## 37. Rhythm-only practice

**Decision: Recommended**

Allow the learner to practise rhythm independently from pitch when needed.

## 38. Metronome and count-in

**Decision: Required / keep**

## 39. Rhythm sight reading

**Decision: Recommended**

Use short unfamiliar rhythmic patterns separately from pitch complexity.

## 40. Tap/keyboard rhythm exercises

**Decision: Recommended**

Avoid requiring a microphone for basic rhythm training when keyboard input is sufficient.

---

# P1 — Technique curriculum

## 41. Five-finger patterns

**Decision: Recommended**

## 42. Scales

**Decision: Recommended**

Introduce gradually with correct fingering explanations and musical use.

## 43. Chords and inversions

**Decision: Recommended**

## 44. Arpeggios

**Decision: Recommended**

## 45. Finger coordination and independence exercises

**Decision: Recommended**

Keep exercises short and tied to a concrete skill rather than adding large amounts of repetitive material without purpose.

## 46. Hanon-style repetitive drills

**Decision: Optional and limited**

Use only when there is a clear technical objective. Do not make repetitive drills the core method.

## 47. Fingering display

**Decision: Recommended**

Show reviewed fingering where useful.

## 48. Progressive fingering removal

**Decision: Recommended**

Once a pattern is learned, reduce unnecessary fingering labels instead of permanently displaying every finger number.

## 49. Posture, relaxation and movement instruction

**Decision: Recommended**

Teach with explanation, original visual demonstrations and self-assessment cues.

## 50. Automatic MIDI posture evaluation

**Decision: Do not build**

MIDI does not provide sufficient evidence to determine posture, wrist position, tension or actual finger use.

## 51. Automatic tension/physical-technique pass/fail from MIDI

**Decision: Do not build**

Physical technique must remain instructional/self-assessed unless a future sensor modality is explicitly researched and validated.

---

# P1 — Chords, accompaniment and practical playing

## 52. Dedicated chord/accompaniment learning path

**Decision: High priority**

Add a clear path for learners who want to accompany songs rather than only play fully notated solo pieces.

Suggested progression:

1. basic triads;
2. chord symbols;
3. inversions;
4. left-hand patterns;
5. broken chords;
6. common accompaniment patterns;
7. lead sheets;
8. simple pop accompaniment;
9. chord progression recognition;
10. basic improvisation.

## 53. Lead-sheet reading

**Decision: Recommended**

Teach melody/chord-symbol reading as a separate practical skill.

## 54. Accompaniment patterns

**Decision: Recommended**

Teach several reusable patterns rather than song-specific memorization only.

## 55. Basic improvisation

**Decision: Recommended later in the path**

Introduce after basic scales, chords, rhythm and ear skills exist.

---

# P1 — Library and imported music

## 56. MusicXML/MXL import

**Decision: Required / keep**

Maintain the existing secure private import path.

## 57. Imported pieces use the same practice engine as bundled pieces

**Decision: Required / keep**

Do not create a second simplified practice architecture for imports.

## 58. Editable practice sections and target tempo

**Decision: Recommended / keep**

## 59. Fingering overrides

**Decision: Recommended**

Allow limited learner overrides without turning Nadiano into a full notation editor.

## 60. Automatically create practice tasks from imported pieces

**Decision: High priority**

Examples:

- difficult-measure loops;
- hands-separate sections;
- tempo ladder;
- rhythm-only drill;
- sight-reading classification when first imported;
- review scheduling.

## 61. Full professional notation editor

**Decision: Do not build for 1.x**

This would greatly expand scope. Keep MusicXML as the interchange format and provide only practice-oriented edits.

---

# P1 — Progress and review system

## 62. Separate skill levels

**Decision: High priority**

Do not represent the learner with only one global level.

Maintain evidence for at least:

- note reading;
- sight reading;
- rhythm;
- timing;
- ear training;
- technique curriculum;
- chords/accompaniment;
- repertoire.

## 63. Review queue / spaced practice

**Decision: High priority / keep improving**

Skills and passages that were previously difficult should return after increasing delays.

## 64. Explain recommendations

**Decision: Required**

Examples:

- "Measure 6 returns because timing was unstable in the last two attempts."
- "Left hand is recommended separately because note accuracy is good but coordination drops with both hands."

## 65. Weekly progress view

**Decision: Recommended**

Show meaningful change, not only total minutes.

## 66. Practice-time statistics

**Decision: Recommended but secondary**

Time should support reflection, not become the main success metric.

## 67. Error trend view

**Decision: Recommended**

Show whether recurring pitch/timing problems are improving over time.

---

# P2 — Motivation and gamification

## 68. Daily goal

**Decision: Recommended**

Keep it flexible and easy to change.

## 69. Personal streak

**Decision: Optional**

If implemented:

- do not punish missed days aggressively;
- allow rest days;
- avoid making streak preservation more important than quality practice.

## 70. Stars / simple completion feedback

**Decision: Optional, restrained**

Useful for quick feedback, especially for beginners.

## 71. Achievements / medals

**Decision: Optional**

Only for meaningful musical milestones, not trivial repeated actions.

## 72. XP and generic player levels

**Decision: Optional, low priority**

If used, they must not replace skill-specific progress.

## 73. Excessive celebration/confetti

**Decision: Do not make central**

Small celebration is acceptable, but the practice screen should remain calm and focused.

## 74. Loot boxes / random reward mechanics

**Decision: Do not build**

No learning value and inappropriate incentive design for this product.

## 75. Global leaderboards

**Decision: Do not build**

They encourage comparison rather than individual musical development.

## 76. Personal challenges

**Decision: Recommended**

Examples:

- complete three sight-reading exercises this week;
- reach target tempo on a current piece;
- review all due interval exercises.

---

# P2 — Coach and explanation layer

## 77. Concrete next-action coach

**Decision: High priority**

Feedback should answer: **What should I do next?**

Examples:

- practise measures 5–6 at 60 BPM;
- repeat left hand twice;
- listen once before replaying;
- switch to rhythm-only mode;
- move on because mastery evidence is sufficient.

## 78. Explain why the coach chose an action

**Decision: Recommended**

Keep recommendations transparent and deterministic where possible.

## 79. Generic AI chat as the main piano teacher

**Decision: Do not make central**

Core musical progression and scoring should remain deterministic, reviewed and testable.

## 80. Optional AI explanations

**Decision: Later / optional**

Could be explored for alternative explanations, summaries or questions, but must not silently override validated lesson content or scoring logic.

---

# P2 — Audio, video and self-review

## 81. Original hand/technique demonstrations

**Decision: Recommended**

Use for concepts MIDI cannot show.

## 82. Slow-motion demonstration

**Decision: Recommended where useful**

Particularly useful for hand movement, fingering changes and coordination.

## 83. Local audio recording

**Decision: Later**

Useful for self-review of musical sound beyond MIDI data.

Privacy requirements:

- opt-in;
- local by default;
- clear deletion;
- no hidden cloud upload.

## 84. Listen back to own performance

**Decision: Later**

Useful for phrasing, balance and musical self-assessment.

## 85. Microphone-based note detection

**Decision: Optional later**

Useful mainly for acoustic pianos. MIDI should remain the preferred input when available.

## 86. Camera-assisted posture/self-review

**Decision: Research later**

Potentially useful, but requires a separate privacy, technical-validity and product-scope decision before implementation.

---

# P2 — Profiles, teachers and social features

## 87. Multiple local learner profiles

**Decision: Required / keep**

## 88. Teacher notes

**Decision: Later**

Could allow a teacher to leave private guidance for a learner profile.

## 89. Teacher-assigned practice

**Decision: Later**

Useful if Nadiano expands beyond household self-learning.

## 90. Teacher progress view

**Decision: Later**

Requires clear privacy and permission rules.

## 91. Live video lessons

**Decision: Do not build as a core Nadiano feature**

Video conferencing is a separate product problem and existing tools already solve it.

## 92. Online multiplayer/group performance

**Decision: Do not prioritize**

High synchronization complexity with little value for the current learning goal.

---

# P1/P2 — UI and interaction backlog

The feature structure above is approved as a direction, but the visual system still requires a dedicated competitive UI review.

## 93. Dedicated UI/visual-style research

**Decision: High priority before a major UI redesign**

Review current desktop/tablet piano-learning products specifically for:

- home/dashboard hierarchy;
- course-map presentation;
- song-library cards and filters;
- practice-screen density;
- notation/control balance;
- result-screen hierarchy;
- progress visualization;
- onboarding;
- MIDI connection state;
- use of color for correct/wrong/current notes;
- typography;
- spacing;
- icon style;
- motion/animation;
- light/dark behavior;
- large tablet/desktop layouts;
- phone layouts;
- accessibility and focus states.

Output should be an explicit Nadiano design specification rather than a vague instruction to "look like" another app.

## 94. Keep top-level navigation small

**Decision: Recommended**

Target five primary learner destinations:

**Today | Learn | Songs | Train | Progress**

Settings, profile and MIDI diagnostics should remain secondary controls.

## 95. Focused practice mode

**Decision: High priority**

While the learner is playing, remove unrelated navigation and visual noise while keeping essential controls immediately accessible.

Essential practice controls include:

- tempo;
- mode;
- loop;
- hand selection;
- metronome/count-in;
- reference playback;
- exit/back.

## 96. Large notation area on desktop/tablet

**Decision: High priority**

The score should receive most of the screen during score-based practice.

## 97. Responsive phone experience

**Decision: Recommended**

Phone screens should remain useful for:

- Today plan;
- progress;
- ear training;
- rhythm drills;
- course navigation;
- setup/diagnostics;
- short lessons.

Do not force a dense full piano score into a phone layout when it becomes unusable.

## 98. Avoid excessive nested navigation

**Decision: Recommended**

Do not create many layers of menus, tabs and modal screens for basic practice actions.

---

# P1 — Product reliability and deployment

## 99. PWA installation

**Decision: Keep**

## 100. Prepared offline practice

**Decision: Keep/improve**

Previously prepared lessons and required assets should remain usable through supported offline interruption scenarios where technically feasible.

## 101. MIDI reconnect

**Decision: Required / keep improving**

Reconnect should be recoverable without losing the current practice state.

## 102. MIDI device diagnostics

**Decision: Required / keep**

Show:

- connected device;
- received notes;
- sustain/sostenuto/soft pedal state;
- browser permission state;
- reconnect controls;
- useful diagnostics export.

## 103. Docker self-hosting

**Decision: Required / keep**

Maintain simple one-container deployment and persistent `/data` storage.

## 104. Cloud requirement

**Decision: Do not introduce**

Nadiano must not require an external cloud service for its core learning loop.

---

# Recommended implementation order

The next product work should focus on learning quality and interaction rather than adding many unrelated features.

1. **UI/visual-style research and Nadiano design specification.**
2. **Today automatic practice plan.**
3. **Focused practice-screen redesign.**
4. **Adaptive one/two-measure mastery flow.**
5. **Progressive removal of learning aids.**
6. **Skill-specific progress and adaptive sight-reading level.**
7. **Dedicated chord/accompaniment path.**
8. **Automatic targeted practice generation for imported pieces.**
9. **Richer demonstrations and optional self-review features.**
10. **Teacher features only after the household/self-learning experience is mature.**

# Explicit non-goals for the foreseeable 1.x line

Unless a future product decision changes them, do not spend development time on:

- falling notes as the primary learning mode;
- global leaderboards;
- loot boxes or aggressive engagement mechanics;
- MIDI-based posture/tension/finger detection claims;
- a full professional notation editor;
- built-in live video conferencing;
- online multiplayer performance;
- a generic AI chatbot controlling the curriculum;
- cloud dependency for core learning.

# Backlog maintenance rule

Before implementing an item from this document:

1. check whether existing Nadiano 1.0 behavior already covers part of it;
2. define the smallest complete learner-facing vertical slice;
3. write objective acceptance criteria;
4. preserve German/Indonesian parity;
5. keep bundled and imported content on the same practice architecture;
6. avoid duplicate abstraction layers;
7. update this backlog when a decision changes or an item is completed.
