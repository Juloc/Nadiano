namespace Nadiano.Core.Progress;

/// <summary>A learner has met a lesson's completion rule (docs/TECHNICAL_ARCHITECTURE.md §11 LessonProgress table).</summary>
public sealed class LessonProgressRecord
{
    public required Guid ProfileId { get; init; }
    public required string CourseId { get; init; }
    public required string LessonId { get; init; }
    public required DateTimeOffset CompletedAtUtc { get; init; }
}