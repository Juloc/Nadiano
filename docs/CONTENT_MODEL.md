# Lesson and content model

## 1. Goals

The content model must:

- support bundled and imported material through the same code path;
- keep notation, learning behavior and translations separate;
- support deterministic validation and migration;
- remain understandable without a custom content platform;
- allow future languages and course levels;
- preserve original source files where legally permitted;
- avoid encoding page-specific UI behavior in lessons.

## 2. Canonical package

A lesson package is a directory in development and an archive when imported or exported.

```text
lesson-id/
  lesson.json
  score.musicxml
  expected-events.json
  reference.mid
  i18n/
    de.json
    id.json
  media/
    technique-top.webm
    technique-side.webm
    reference.ogg
  attribution.json
```

Only `lesson.json` and at least one localized title are always required. Score, expected events and media are required according to the lesson type.

## 3. Content identifiers

Identifiers are lowercase kebab-case and immutable after publication.

Examples:

```text
foundation-natural-hand-01
rhythm-quarter-half-02
reading-landmark-treble-g-01
piece-morning-steps-01
```

A renamed title does not change the identifier. Imported private content uses a generated UUID as its stable internal identity and may also retain a readable slug.

## 4. Lesson manifest

Initial schema:

```json
{
  "schemaVersion": 1,
  "id": "foundation-light-thumb-01",
  "kind": "guided-lesson",
  "stage": "F0",
  "order": 60,
  "estimatedMinutes": 8,
  "skills": ["body.thumb-side-contact", "technique.release"],
  "prerequisites": ["foundation-natural-hand-01"],
  "notation": {
    "path": "score.musicxml",
    "partMapping": {
      "P1": "right-hand"
    }
  },
  "practice": {
    "supportedModes": ["wait", "loop", "performance"],
    "defaultMode": "wait",
    "targetTempo": 56,
    "countInMeasures": 1,
    "sections": [
      {
        "id": "pattern-a",
        "fromMeasure": 1,
        "toMeasure": 2,
        "repetitions": 2
      }
    ]
  },
  "assessment": {
    "categories": ["pitch", "onset", "duration"],
    "completionRule": {
      "requiredSuccessfulRuns": 2,
      "maximumPitchErrors": 0,
      "minimumTimingScore": 0.75
    },
    "selfChecks": [
      "technique.thumb-loose",
      "body.wrist-neutral"
    ]
  },
  "review": {
    "initialIntervalsDays": [1, 3, 7]
  },
  "localization": {
    "directory": "i18n"
  },
  "attribution": "attribution.json"
}
```

The schema should use explicit fields rather than a generic properties bag. New behavior requires a schema change, validator and migration note.

## 5. Localization file

```json
{
  "title": "Leichter Daumen",
  "summary": "Spiele mit einem lockeren, seitlich aufliegenden Daumen.",
  "why": "Ein freier Daumen erleichtert gleichmäßige Übergänge und Positionswechsel.",
  "steps": [
    "Lege den Daumen seitlich auf eine Taste.",
    "Spiele langsam und lasse die Schulter locker.",
    "Hebe die Hand nach dem Muster entspannt an."
  ],
  "commonMistake": "Der Daumen liegt flach unter der Hand und zieht das Handgelenk nach unten.",
  "successMessage": "Das Muster war kontrolliert. Prüfe nun, ob der Daumen locker blieb."
}
```

Localization files contain prose only. Timing tolerances, scores and behavior stay in the manifest.

## 6. Skill catalogue

Skills are defined centrally in a versioned catalogue:

```json
{
  "id": "technique.release",
  "competency": "technique",
  "introducedStage": "F0",
  "measurability": "self-assessment",
  "relatedSkills": ["body.shoulders-relaxed"]
}
```

Allowed measurability values:

- `midi-objective`;
- `audio-assisted`;
- `self-assessment`;
- `teacher-review`;
- `mixed`.

The application must not infer an objective pass for a skill marked only as self-assessment.

## 7. Expected events

The scoring engine consumes a normalized expected-event document rather than directly interpreting rendered SVG.

```json
{
  "schemaVersion": 1,
  "timeBase": "beats",
  "tempoMap": [
    { "beat": 0, "bpm": 56 }
  ],
  "events": [
    {
      "id": "m1-v1-n1",
      "measure": 1,
      "beat": 0,
      "durationBeats": 1,
      "pitches": [60],
      "hand": "right",
      "voice": "1",
      "fingering": [1],
      "articulation": "legato",
      "velocityTarget": {
        "minimum": 38,
        "maximum": 76
      }
    }
  ]
}
```

Expected events may be generated from MusicXML during build or import, then validated and stored. The original MusicXML remains canonical for notation.

## 8. Fingering

Fingering should be stored in MusicXML where representable. The normalized event model mirrors it for practice display and validation.

Rules:

- imported fingering is preserved but marked as imported;
- automatically proposed fingering is never treated as reviewed;
- bundled lessons require human-reviewed fingering;
- alternate fingering may be represented with explicit variants;
- fingering is instructional display data, not a MIDI-measured result.

## 9. Technique demonstrations

Technique media metadata:

```json
{
  "id": "light-thumb",
  "views": [
    {
      "kind": "top",
      "path": "media/technique-top.webm",
      "poster": "media/technique-top.webp"
    },
    {
      "kind": "side",
      "path": "media/technique-side.webm",
      "poster": "media/technique-side.webp"
    }
  ],
  "hasAudioDescription": true,
  "loop": true
}
```

Use short purpose-built media, not long embedded lectures. Provide text alternatives and reduced-motion behavior.

## 10. Lesson kinds

Initial kinds:

- `guided-lesson`;
- `technique-drill`;
- `rhythm-drill`;
- `reading-drill`;
- `ear-drill`;
- `mini-piece`;
- `repertoire-piece`;
- `stage-check`.

Later kinds:

- `improvisation-task`;
- `theory-task`;
- `recording-review`;
- `teacher-assignment`.

Each kind has its own required fields and validator. Avoid a single manifest with dozens of nullable properties.

## 11. Generated exercises

Generated reading and rhythm tasks use reviewed templates, not unrestricted generative output.

Template example:

```json
{
  "id": "reading-steps-landmarks-level-1",
  "clefs": ["treble", "bass"],
  "landmarks": ["treble-g", "middle-c", "bass-f"],
  "intervals": [0, 1, 2],
  "measureCount": 4,
  "meters": ["4/4"],
  "rhythms": ["quarter", "half"],
  "range": {
    "minimumMidi": 48,
    "maximumMidi": 72
  },
  "seeded": true
}
```

Every generated task records its seed so a result is reproducible.

## 12. Course manifest

```json
{
  "schemaVersion": 1,
  "id": "nadiano-foundations",
  "version": "0.1.0",
  "defaultLanguage": "de",
  "supportedLanguages": ["de", "id"],
  "stages": [
    {
      "id": "F0",
      "titleKey": "course.stage.f0",
      "items": [
        "foundation-keyboard-map-01",
        "foundation-sitting-01",
        "foundation-release-01"
      ]
    }
  ]
}
```

Course versions follow semantic versioning for content compatibility:

- patch: typo or media correction without progression changes;
- minor: additional compatible lessons or optional paths;
- major: progression or schema changes requiring migration or re-evaluation.

## 13. Import workflow

### MusicXML/MXL

1. Store original upload in a temporary import area.
2. Parse with strict size and entity limits.
3. Validate supported parts, measures and notation.
4. Present warnings for unsupported or ambiguous features.
5. Ask for right/left hand and voice mapping when needed.
6. Extract or propose expected events.
7. Review tempo, fingering and practice sections.
8. Add localized metadata.
9. Run the same validator used for bundled content.
10. Publish to the profile's private library.

### MIDI

MIDI import is not part of the first alpha. Later it requires:

- tempo and meter selection;
- quantization review;
- split-point or part assignment;
- enharmonic spelling review;
- tie and voice correction;
- fingering entry;
- MusicXML export and final validation.

A raw MIDI upload must never silently become a scored notation lesson.

## 14. Validation

Validation levels:

1. **Schema validation** — required fields and types.
2. **Reference validation** — referenced files, skills and prerequisites exist.
3. **Music validation** — measures, tempo, pitches and event IDs are consistent.
4. **Pedagogy validation** — stage, prerequisite and assessment choices are plausible.
5. **Localization validation** — required languages and keys exist.
6. **License validation** — attribution and source declarations are complete.
7. **Runtime validation** — lesson can render and generate a practice session.

Bundled content fails the build on errors. User imports show actionable errors without crashing the app.

## 15. Content storage

- Bundled content is read-only in the container image.
- Imported content is stored under the persistent data volume.
- Database rows store identities, versions, ownership and progress, not large MusicXML/media blobs unless a later measured need justifies it.
- User content paths are generated by the application and never derived directly from untrusted filenames.
- Export packages contain manifest, source files, translations and attribution, but not unrelated profile history.

## 16. Security limits

- maximum upload size is configurable and conservative;
- MXL archives reject path traversal, nested archive expansion and excessive file counts;
- XML parsing disables external entities;
- media is checked by type and served from controlled paths;
- HTML is not accepted in translation fields;
- imported scripts are never executed;
- package IDs cannot overwrite bundled content.

## 17. Migration

Every schema version has:

- a JSON schema or equivalent validator;
- a migration function to the next supported version where feasible;
- fixtures for valid and invalid examples;
- documentation in an architecture decision when behavior changes.

The app keeps the original imported package until migration succeeds and records the applied migration version.
