namespace Nadiano.Core.Content.Manifests;

/// <summary>
/// Deserialized lesson.json. Mirrors docs/CONTENT_MODEL.md §4 with explicit
/// fields rather than a generic properties bag.
/// </summary>
public sealed class LessonManifest
{
    public required int SchemaVersion { get; init; }
    public required string Id { get; init; }
    public required LessonKind Kind { get; init; }
    public required string Stage { get; init; }
    public required int Order { get; init; }
    public required int EstimatedMinutes { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<string> Prerequisites { get; init; } = [];
    public NotationReference? Notation { get; init; }
    public PracticeConfig? Practice { get; init; }
    public AssessmentConfig? Assessment { get; init; }
    public ReviewConfig? Review { get; init; }
    public required LocalizationConfig Localization { get; init; }
    public string? Attribution { get; init; }
}

public sealed class NotationReference
{
    public required string Path { get; init; }

    /// <summary>MusicXML part id to a free-form role description (e.g. "right-hand").</summary>
    public IReadOnlyDictionary<string, string> PartMapping { get; init; } = new Dictionary<string, string>();
}

public sealed class PracticeConfig
{
    public required IReadOnlyList<PracticeMode> SupportedModes { get; init; }
    public required PracticeMode DefaultMode { get; init; }
    public required int TargetTempo { get; init; }
    public int CountInMeasures { get; init; }
    public IReadOnlyList<PracticeSection> Sections { get; init; } = [];
}

public sealed class PracticeSection
{
    public required string Id { get; init; }
    public required int FromMeasure { get; init; }
    public required int ToMeasure { get; init; }
    public int Repetitions { get; init; } = 1;
}

public sealed class AssessmentConfig
{
    public required IReadOnlyList<AssessmentCategory> Categories { get; init; }
    public required CompletionRule CompletionRule { get; init; }
    public IReadOnlyList<string> SelfChecks { get; init; } = [];
}

public sealed class CompletionRule
{
    public required int RequiredSuccessfulRuns { get; init; }
    public int MaximumPitchErrors { get; init; }
    public double MinimumTimingScore { get; init; }
}

public sealed class ReviewConfig
{
    public IReadOnlyList<int> InitialIntervalsDays { get; init; } = [];
}

public sealed class LocalizationConfig
{
    public required string Directory { get; init; }
}