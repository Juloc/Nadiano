# Research basis and sources

Last reviewed: 2026-08-04.

This document records why Nadiano's curriculum and architecture use particular principles. It is not a substitute for ongoing review by qualified piano teachers, musicians, accessibility specialists and security reviewers.

## 1. International curriculum references

### ABRSM piano syllabuses

Sources:

- [ABRSM syllabuses](https://www.abrsm.org/en-bz/syllabuses)
- [ABRSM Piano Syllabus 2027 & 2028 announcement](https://www.abrsm.org/en-us/news/new-piano-syllabus-and-free-learning-resources-available-now)

Relevant design conclusions:

- repertoire alone is not a complete practical-musicianship path;
- scales/arpeggios, sight-reading and aural work remain distinct competencies;
- progression should support Initial Grade through advanced grade concepts without copying examination material;
- Nadiano should map internal competencies to external frameworks only as guidance, not claim examination certification.

The 2027–2028 ABRSM update changes repertoire while retaining supporting tests. This supports keeping Nadiano's competence model separate from individual repertoire lists.

### Royal Conservatory of Music

Source:

- [RCM Piano Syllabus: 2022 Edition and errata](https://teacherportal.rcmusic.com/Resources/Syllabus-Piano)

Relevant design conclusions:

- technical requirements, repertoire, ear tests and sight-reading should be represented separately;
- the long-term course model should extend from preparatory levels through advanced study;
- stage outcomes and technical breadth are more useful than a single linear song list.

### Trinity College London

Source:

- [Trinity piano resources and syllabuses](https://www.trinitycollege.com/qualifications/music/grade-exams/piano/resources)

Relevant design conclusions:

- improvisation, musical knowledge and creative options deserve explicit curriculum space;
- performance and supporting skills may be configured in different valid combinations;
- Nadiano should include creative application rather than treating correct reproduction as the only outcome.

## 2. Piano-method design references

### Faber Piano Adventures

Sources:

- [Primer Technique & Artistry support](https://pianoadventures.com/qr/ff1096/)
- [Accelerated Level 1 teaching notes](https://pianoadventures.com/piano-books/accelerated-piano-adventures/level-1/accelerated-1-qa/)
- [Level 2A Technique & Artistry description](https://pianoadventures.com/product/piano-adventures-level-2a-technique-artistry-book-2nd-edition/)

Relevant design conclusions:

- teach body, arm, wrist and finger as a coordinated system;
- introduce memorable focused technique concepts such as rounded hand shape, arm support, light thumb and wrist release;
- reinforce each physical concept through an exercise and musical application;
- use demonstrations and imitation, not text alone.

Nadiano must write original explanations, exercises, media and music. These references inform teaching structure and topic coverage, not copyable product content.

### Alfred and other beginner methods

Official method overviews commonly begin with keyboard orientation, finger numbers, seating/hand preparation and simple rhythmic/aural work before dense notation. Nadiano uses this broad pedagogical pattern while avoiding permanent positional reading such as treating one finger as permanently assigned to one pitch.

### Intervallic and auditory approaches

Method systems that combine rote/auditory pieces with intervallic reading demonstrate an important product principle: hearing, pattern recognition and notation should support one another. Nadiano therefore includes listen-and-copy, contour, landmark and interval tasks alongside note naming.

## 3. Practice and feedback research

### Deliberate and task-specific practice

Source:

- Platz et al., [The influence of deliberate practice on musical achievement: a meta-analysis](https://pmc.ncbi.nlm.nih.gov/articles/PMC4073287/)

Relevant design conclusions:

- accumulated repetition alone is not the product goal;
- tasks should target a defined weakness and provide corrective information;
- sight-reading, listening and performance require task-specific practice rather than assuming repertoire repetition transfers completely;
- Nadiano should recommend a specific next action and selected passage.

The research does not justify claiming that software-selected deliberate practice guarantees expertise. Achievement depends on more than measured practice quantity.

### Auditory feedback

Sources:

- [The role of auditory feedback in the motor learning of music in experienced and novice performers](https://pmc.ncbi.nlm.nih.gov/articles/PMC9671877/)
- [The influence of pitch feedback on learning of motor-timing and sequencing: a piano study with novices](https://pmc.ncbi.nlm.nih.gov/articles/PMC6261582/)
- [Effects of audio feedback interventions with the Disklavier on the performance of piano students](https://pmc.ncbi.nlm.nih.gov/articles/PMC12078165/)

Relevant design conclusions:

- reference listening and playback comparison should be part of the learning loop;
- normal sound feedback is important, especially in novice motor-sequence learning;
- listening tasks should not be treated as optional decoration;
- audio feedback can support review but should not be presented as a universal automatic correction method.

### Expressive practice

Source:

- [Independent practice approaches for expressive piano performance: modeling, structural understanding, and narrative imagery](https://pubmed.ncbi.nlm.nih.gov/42338554/)

Relevant design conclusions:

- advanced practice should include listening/modeling and structural understanding, not only note-level correction;
- the long-term curriculum needs analysis, interpretation and reflective recording work;
- a future advanced product must avoid implying that timing/velocity measurements fully represent expression.

## 4. Technical standards

### Web MIDI

Sources:

- [W3C Web MIDI API](https://www.w3.org/TR/webmidi/)
- [MDN Web MIDI API](https://developer.mozilla.org/en-US/docs/Web/API/Web_MIDI_API)

Relevant design conclusions:

- MIDI device access belongs in the learner's browser;
- a secure context and explicit user permission are required;
- browser availability is limited and must be capability-tested;
- permission may be restricted by browser policy;
- Chrome/Edge-compatible desktop browsers are the first supported target;
- the Docker server cannot directly solve unsupported client-browser MIDI access.

### MusicXML

Sources:

- [MusicXML 4.0 fingering element](https://www.w3.org/2021/06/musicxml40/musicxml-reference/elements/fingering/)
- [MusicXML technical element](https://www.w3.org/2021/06/musicxml40/musicxml-reference/elements/technical/)
- [MusicXML notations element](https://www.w3.org/2021/06/musicxml40/musicxml-reference/elements/notations/)

Relevant design conclusions:

- MusicXML/MXL is appropriate as the canonical notation exchange format;
- fingering can be represented under note notations/technical information;
- alternate and substitution fingerings are representable;
- imported fingering still needs pedagogical review;
- raw MIDI is not a replacement for reviewed notation, spelling, voices and fingering.

### OpenSheetMusicDisplay

Source:

- [OpenSheetMusicDisplay repository](https://github.com/opensheetmusicdisplay/opensheetmusicdisplay)

Status reviewed in August 2026:

- the project renders MusicXML in browsers through a TypeScript-accessible API;
- SVG output and modifiable score data suit highlighting and responsive display;
- version 1.9.9 was the latest listed release when this document was reviewed;
- the project is a renderer, not a complete Nadiano lesson or notation editor.

Relevant design conclusions:

- use OSMD behind a Nadiano notation adapter;
- pin the version and serve it locally;
- keep expected-event and lesson logic independent from rendered SVG internals;
- do not promise a full score editor in the beta.

### .NET support

Source:

- [Microsoft .NET releases and support](https://learn.microsoft.com/dotnet/core/releases-and-support)

Status reviewed in August 2026:

- .NET 10 is an LTS release supported until November 2028;
- ASP.NET Core and EF Core follow the .NET lifecycle.

Relevant design conclusions:

- start on .NET 10 LTS;
- use the platform's Razor Pages, localization, health checks and EF Core support before adding third-party frameworks;
- document deliberate runtime upgrades rather than floating without review.

## 5. Measurement limitations

The following are product constraints, not missing implementation tasks:

| Property | MIDI-only confidence | Product treatment |
|---|---:|---|
| pitch | high | objective score |
| onset time | high within device/clock limits | objective score with tolerance |
| note-off/duration | moderate to high | objective observation |
| key velocity | high as MIDI value | dynamic proxy, not acoustic tone quality |
| sustain controller | high as event | event observation plus acoustic self-check |
| actual finger used | unavailable | display prescribed fingering and self-check |
| posture | unavailable | explanation, demonstration, self/teacher review |
| muscle tension | unavailable | safety reminder and self/teacher review |
| wrist/arm motion | unavailable | demonstration and later optional video review |
| room sound | unavailable | later microphone/audio workflow |
| interpretation | partial proxies only | listening, reflection and teacher input |

## 6. Safety and teaching limits

- pain, numbness or persistent discomfort must result in a stop-and-seek-qualified-help message, not an automatic correction routine;
- advanced octave, repeated-note and high-force work should not be assigned without appropriate preparation and human review;
- technique explanations should avoid rigid universal measurements of hand shape because anatomy varies;
- the app should encourage short focused practice and breaks rather than reward harmful duration;
- professional preparation must explicitly recommend qualified teacher guidance.

## 7. Open research and validation tasks

Before the beta content is finalized:

1. Obtain review from at least one qualified piano teacher for the F0–B2 progression.
2. Obtain native Indonesian language review from a musically informed reviewer.
3. Test terminology and instructions with both adult learners.
4. Compare scoring tolerances across at least two real MIDI pianos.
5. Evaluate whether the technique animations cause correct imitation without a teacher prompt.
6. Test whether one-cue feedback is more understandable than multi-cue feedback in the product.
7. Review public-domain status and Nadiano edition ownership for every bundled melody.
8. Recheck browser and dependency support before beta and 1.0.

## 8. Evidence policy

Nadiano documentation should distinguish:

- formal examination requirements;
- established teaching-method practice;
- peer-reviewed empirical evidence;
- product design inference;
- unvalidated future idea.

Do not use “scientifically proven” unless a narrowly defined statement is supported by appropriate evidence. Product rules based partly on judgment must be labeled as design decisions and tested with learners.
