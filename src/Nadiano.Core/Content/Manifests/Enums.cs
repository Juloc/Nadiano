using System.Text.Json.Serialization;

namespace Nadiano.Core.Content.Manifests;

// Kebab-case values per docs/CONTENT_MODEL.md §10, mapped explicitly rather than
// via a generic naming-policy converter so the allowed set stays visible here.
[JsonConverter(typeof(JsonStringEnumConverter<LessonKind>))]
public enum LessonKind
{
    [JsonStringEnumMemberName("guided-lesson")]
    GuidedLesson,

    [JsonStringEnumMemberName("technique-drill")]
    TechniqueDrill,

    [JsonStringEnumMemberName("rhythm-drill")]
    RhythmDrill,

    [JsonStringEnumMemberName("reading-drill")]
    ReadingDrill,

    [JsonStringEnumMemberName("ear-drill")]
    EarDrill,

    [JsonStringEnumMemberName("mini-piece")]
    MiniPiece,

    [JsonStringEnumMemberName("repertoire-piece")]
    RepertoirePiece,

    [JsonStringEnumMemberName("stage-check")]
    StageCheck,
}

[JsonConverter(typeof(JsonStringEnumConverter<PracticeMode>))]
public enum PracticeMode
{
    [JsonStringEnumMemberName("explore")]
    Explore,

    [JsonStringEnumMemberName("wait")]
    Wait,

    [JsonStringEnumMemberName("rhythm")]
    Rhythm,

    [JsonStringEnumMemberName("hands-separate")]
    HandsSeparate,

    [JsonStringEnumMemberName("loop")]
    Loop,

    [JsonStringEnumMemberName("tempo-ladder")]
    TempoLadder,

    [JsonStringEnumMemberName("listen-and-copy")]
    ListenAndCopy,

    [JsonStringEnumMemberName("performance")]
    Performance,

    [JsonStringEnumMemberName("sight-reading")]
    SightReading,

    [JsonStringEnumMemberName("free-practice")]
    FreePractice,
}

[JsonConverter(typeof(JsonStringEnumConverter<AssessmentCategory>))]
public enum AssessmentCategory
{
    [JsonStringEnumMemberName("pitch")]
    Pitch,

    [JsonStringEnumMemberName("onset")]
    Onset,

    [JsonStringEnumMemberName("duration")]
    Duration,

    [JsonStringEnumMemberName("steadiness")]
    Steadiness,

    [JsonStringEnumMemberName("articulation")]
    Articulation,

    [JsonStringEnumMemberName("dynamics")]
    Dynamics,

    [JsonStringEnumMemberName("pedal")]
    Pedal,
}

// docs/LEARNING_CURRICULUM.md §2 competency codes.
[JsonConverter(typeof(JsonStringEnumConverter<CompetencyArea>))]
public enum CompetencyArea
{
    [JsonStringEnumMemberName("body")]
    Body,

    [JsonStringEnumMemberName("technique")]
    Technique,

    [JsonStringEnumMemberName("rhythm")]
    Rhythm,

    [JsonStringEnumMemberName("reading")]
    Reading,

    [JsonStringEnumMemberName("ear")]
    Ear,

    [JsonStringEnumMemberName("theory")]
    Theory,

    [JsonStringEnumMemberName("expression")]
    Expression,

    [JsonStringEnumMemberName("creative")]
    Creative,

    [JsonStringEnumMemberName("repertoire")]
    Repertoire,

    [JsonStringEnumMemberName("practice")]
    Practice,

    [JsonStringEnumMemberName("performance")]
    Performance,
}

[JsonConverter(typeof(JsonStringEnumConverter<SkillMeasurability>))]
public enum SkillMeasurability
{
    [JsonStringEnumMemberName("midi-objective")]
    MidiObjective,

    [JsonStringEnumMemberName("audio-assisted")]
    AudioAssisted,

    [JsonStringEnumMemberName("self-assessment")]
    SelfAssessment,

    [JsonStringEnumMemberName("teacher-review")]
    TeacherReview,

    [JsonStringEnumMemberName("mixed")]
    Mixed,
}

// Values match docs/CONTENT_MODEL.md §7 expected-events example ("hand": "right").
// Note.partMapping (§4) uses free-form role strings, not this enum.
[JsonConverter(typeof(JsonStringEnumConverter<Hand>))]
public enum Hand
{
    [JsonStringEnumMemberName("left")]
    Left,

    [JsonStringEnumMemberName("right")]
    Right,

    [JsonStringEnumMemberName("both")]
    Both,
}

[JsonConverter(typeof(JsonStringEnumConverter<ArticulationKind>))]
public enum ArticulationKind
{
    [JsonStringEnumMemberName("legato")]
    Legato,

    [JsonStringEnumMemberName("detached")]
    Detached,

    [JsonStringEnumMemberName("staccato")]
    Staccato,
}