using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Courses;

public enum LessonAvailability
{
    Locked,
    Available,
    Completed,
}

public sealed record LessonProgressEntry(
    string LessonId,
    string StageId,
    LessonAvailability Availability,
    IReadOnlyList<string> MissingPrerequisites);

/// <summary>
/// Computes lesson lock/unlock state from a course manifest and a learner's
/// completed lessons. Locking is based solely on the explicit
/// LessonManifest.Prerequisites field (docs/CONTENT_MODEL.md §4), not on
/// implicit stage or item order (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-019).
/// </summary>
public static class CourseProgressCalculator
{
    public static IReadOnlyList<LessonProgressEntry> Calculate(
        CourseManifest course,
        IReadOnlyDictionary<string, LessonManifest> lessonsById,
        IReadOnlySet<string> completedLessonIds)
    {
        var entries = new List<LessonProgressEntry>();

        foreach (var stage in course.Stages)
        {
            foreach (var lessonId in stage.Items)
            {
                if (!lessonsById.TryGetValue(lessonId, out var lesson))
                {
                    // Manifest references a lesson that isn't loaded (e.g. content not
                    // bundled yet); nothing to report for it.
                    continue;
                }

                if (completedLessonIds.Contains(lessonId))
                {
                    entries.Add(new LessonProgressEntry(lessonId, stage.Id, LessonAvailability.Completed, []));
                    continue;
                }

                var missing = lesson.Prerequisites.Where(p => !completedLessonIds.Contains(p)).ToArray();
                var availability = missing.Length == 0 ? LessonAvailability.Available : LessonAvailability.Locked;
                entries.Add(new LessonProgressEntry(lessonId, stage.Id, availability, missing));
            }
        }

        return entries;
    }

    /// <summary>First lesson a learner could start right now, in course order.</summary>
    public static string? RecommendNextLesson(IReadOnlyList<LessonProgressEntry> entries) =>
        entries.FirstOrDefault(entry => entry.Availability == LessonAvailability.Available)?.LessonId;
}