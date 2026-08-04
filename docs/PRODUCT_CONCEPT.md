# Product concept

## 1. Purpose

Nadiano is a self-hosted piano learning web application for households, initially for two learners. It connects the learner's browser to a digital piano through USB MIDI and combines guided lessons, notation, listening, technique reminders, deliberate practice and measurable feedback.

The product is designed for:

- complete beginners who need a correct physical and musical foundation;
- returning learners who need structured practice;
- intermediate learners who want stronger reading, rhythm, ear and technique;
- advanced learners who need preparation skills, while understanding that professional development requires qualified human instruction.

Nadiano is not a falling-note game and not a replacement for a piano teacher. It is a structured practice and learning system.

## 2. Product principles

### 2.1 Learn in parallel

Every stage develops multiple competencies together:

- physical technique and coordination;
- keyboard orientation;
- rhythm and pulse;
- reading in treble and bass clefs;
- listening and imitation;
- theory and harmony;
- repertoire and musical expression;
- sight-reading;
- improvisation and creative application;
- practice skills and self-evaluation.

A learner must not complete a stage by memorizing a few pieces while being unable to read, hear or understand them.

### 2.2 Explain, demonstrate, apply and revisit

Every new technique follows this sequence:

1. Explain what the movement is for.
2. Show it from useful angles.
3. Contrast a correct and common incorrect version.
4. Rehearse the movement without musical complexity.
5. Apply it to a short MIDI-measurable pattern.
6. Apply it in a musical phrase or piece.
7. Ask for a brief self-check of non-MIDI aspects.
8. Revisit it later in another context.

### 2.3 Measure only what can be measured

MIDI can objectively provide:

- played pitch;
- note-on time;
- note-off time;
- velocity;
- pedal and other supported control events;
- simultaneity and ordering.

MIDI cannot reliably provide:

- the actual finger used;
- posture or seating position;
- muscle tension;
- wrist, arm or shoulder movement;
- acoustic tone quality in the room;
- musical intent.

The interface must label objective feedback and learner self-assessment separately. Camera or teacher review may later supplement, but never silently replace, this boundary.

### 2.4 Feedback must produce the next action

Results are separated into pitch, rhythm, steadiness, duration, articulation, dynamics and pedal. The app identifies the measure or beat involved and proposes one next action, such as:

- repeat measures 3–4 at 50 BPM;
- practise the left hand alone;
- clap the rhythm before playing;
- listen and copy the phrase once;
- repeat with the technique cue “loose wrist”.

An unexplained combined percentage is not sufficient.

### 2.5 Content is data

Bundled courses and user imports run through the same content loader, validators and practice engine. Course behavior must not be hard-coded into pages.

MusicXML/MXL is the canonical notation format. MIDI may be used as reference playback or imported raw performance data, but a MIDI file alone is not treated as a finished lesson because notation, spelling, hands, voices and fingering require review.

## 3. Primary user journeys

### 3.1 First start

1. Select interface language.
2. Create or select a learner profile.
3. Run the MIDI compatibility check.
4. Select the digital piano input.
5. Confirm sustain pedal behavior.
6. Complete a short placement and orientation flow.
7. Start the recommended first session.

The learner must still be able to explore non-MIDI theory, listening and reading demonstrations when no compatible MIDI device is available.

### 3.2 Daily session

A recommended session combines:

- a short physical preparation cue;
- one technique or coordination task;
- one reading or rhythm task;
- one listening task;
- current repertoire work;
- a short review item selected from previous weaknesses.

The learner can shorten the session, but the app explains which competency is being skipped.

### 3.3 Guided practice

1. Read and watch the lesson goal.
2. Preview the score and fingering.
3. Listen to a reference where appropriate.
4. Practise in wait, rhythm, loop or performance mode.
5. Receive category-specific feedback.
6. Repeat only the necessary part.
7. Complete a technique self-check.
8. Save the result and next recommendation.

### 3.4 Importing material

1. Upload MusicXML or MXL.
2. Validate notation and unsupported elements.
3. Review parts, hands, voices, tempo and fingering.
4. Define practice sections and goals.
5. Optionally attach a reference MIDI file.
6. Add localized title and instructions.
7. Save as a private lesson package.

MIDI import is a later workflow with explicit quantization and notation review.

## 4. Practice modes

- **Explore:** free playing with a live keyboard and event monitor.
- **Wait:** progression pauses until the required pitch or chord is played.
- **Rhythm:** pitch may be simplified while onset and pulse are assessed.
- **Hands separate:** practise an isolated hand or voice.
- **Loop:** repeat a selected measure range with optional count-in.
- **Tempo ladder:** raise tempo only after configured successful repetitions.
- **Listen and copy:** hear a short phrase and reproduce it.
- **Performance:** play through without interruption, then review.
- **Sight-reading:** one preview period, one attempt, no rewinding.
- **Free practice:** record a performance without predefined expected notes.

Not every mode is available in the alpha.

## 5. Profiles and privacy

Each learner profile contains independent:

- language and notation preferences;
- MIDI device preference;
- course progress;
- practice history;
- accessibility settings;
- target session length;
- imported private material.

The first release uses local household profiles, not internet accounts. The application must support data export, backup and profile deletion. Audio or video recording is opt-in and excluded from the first alpha.

## 6. Languages and notation systems

Initial languages:

- German (`de`);
- Indonesian (`id`).

The localization model must support additional languages without changing lesson logic. The notation preference is independent from interface language and can later include:

- German note names, including H and B;
- international letter names, including B and B-flat;
- solfège;
- Indonesian numbered notation as a later display or exercise mode.

## 7. Scope by maturity

### Alpha

Prove the complete learning loop with a small number of original foundation lessons and a reliable MIDI/scoring path.

### Beta

Provide a coherent beginner course, adaptive practice and a safe content import workflow suitable for daily household use.

### 1.0

Provide a stable, documented, accessible and maintainable self-hosted product with complete beginner outcomes and extension points for intermediate content.

### Later

- expanded intermediate and advanced curricula;
- MIDI-to-MusicXML assisted import;
- teacher review and comments;
- audio recording and acoustic comparison;
- optional camera-assisted self-review;
- cloud synchronization;
- shared or community content after licensing and moderation systems exist.

## 8. Explicit non-goals for 1.0

- automatic diagnosis of physical technique from MIDI;
- professional conservatory replacement;
- public social network;
- competitive global leaderboards;
- marketplace or copyrighted score distribution;
- microservice architecture;
- native mobile applications;
- automatic fingering claims without user review;
- generated pedagogical content published without human validation.

## 9. Success criteria

A successful 1.0 learner can:

- set up and use a USB MIDI piano without technical assistance;
- understand finger numbers and basic healthy setup cues;
- maintain a steady basic pulse;
- read elementary material in both clefs using landmarks and intervals;
- coordinate simple two-hand textures;
- hear and reproduce short patterns;
- use basic dynamics, articulation and sustain pedal deliberately;
- practise a difficult passage using loops, hands-separate work and tempo reduction;
- explain what to practise next instead of only replaying an entire piece;
- export and restore their own learning data.
