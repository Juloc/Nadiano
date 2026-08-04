namespace Nadiano.Core.Content.Manifests;

/// <summary>Deserialized course.json (docs/CONTENT_MODEL.md §12).</summary>
public sealed class CourseManifest
{
    public required int SchemaVersion { get; init; }
    public required string Id { get; init; }
    public required string Version { get; init; }
    public required string DefaultLanguage { get; init; }
    public required IReadOnlyList<string> SupportedLanguages { get; init; }
    public required IReadOnlyList<CourseStage> Stages { get; init; }
}

public sealed class CourseStage
{
    public required string Id { get; init; }
    public required string TitleKey { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
}