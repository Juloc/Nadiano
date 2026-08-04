namespace Nadiano.Core.Content.Manifests;

/// <summary>
/// Deserialized i18n/{culture}.json. Prose only — timing, scores and
/// behavior stay in lesson.json (docs/CONTENT_MODEL.md §5).
/// </summary>
public sealed class LocalizedLessonText
{
    public required string Title { get; init; }
    public string? Summary { get; init; }
    public string? Why { get; init; }
    public IReadOnlyList<string> Steps { get; init; } = [];
    public string? CommonMistake { get; init; }
    public string? SuccessMessage { get; init; }
}