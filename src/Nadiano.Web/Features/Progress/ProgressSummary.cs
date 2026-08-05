namespace Nadiano.Web.Features.Progress;

public sealed record ProgressSummary(
    int CompletedLessonCount,
    int TotalLessonCount,
    IReadOnlyList<CompetencyAreaProgress> Competencies,
    IReadOnlyList<CategoryObservation> CategoryObservations,
    IReadOnlyList<RecentPracticeItem> RecentPractice,
    IReadOnlyList<CompletedLessonItem> RecentCompletions,
    IReadOnlyList<ReviewDueItem> ReviewsDue,
    LessonRecommendation? RecommendedLesson,
    string? LatestNextActionCode);

public sealed record CompetencyAreaProgress(
    string AreaCode,
    int CompletedLessonCount,
    int TotalLessonCount,
    int CompletionPercent);

public sealed record CategoryObservation(
    string CategoryCode,
    int SampleCount,
    int? AccuracyPercent,
    string? TrendCode);

public sealed record RecentPracticeItem(
    string LessonId,
    string LessonTitle,
    DateTimeOffset CompletedAtUtc,
    string NextActionCode);

public sealed record CompletedLessonItem(
    string LessonId,
    string LessonTitle,
    DateTimeOffset CompletedAtUtc);

public sealed record ReviewDueItem(
    string LessonId,
    string LessonTitle,
    DateTimeOffset DueAtUtc);

public sealed record LessonRecommendation(string LessonId, string LessonTitle);
