using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;
using Nadiano.Core.Courses;
using Nadiano.Core.Progress;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.Infrastructure.Courses;

public sealed record CourseProgressMap(
    CourseManifest Course,
    IReadOnlyList<LessonProgressEntry> Entries,
    string? RecommendedNextLessonId);

/// <summary>
/// Combines the database (enrollment, completed lessons, attempts) with the
/// bundled content catalogue to answer course-progression questions
/// (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-019).
/// </summary>
public sealed class CourseProgressService(NadianoDbContext db, ContentCatalogue catalogue)
{
    public async Task<CourseProgressMap?> GetCourseMapAsync(
        Guid profileId, string courseId, CancellationToken cancellationToken = default)
    {
        if (!catalogue.TryGetCourse(courseId, out var course, out var lessonsById))
        {
            return null;
        }

        // Viewing a course's map is what marks it as started, since there is
        // no separate "enroll" action in the UI.
        await EnsureEnrolledAsync(profileId, courseId, cancellationToken);

        var completedRows = await db.LessonProgress
            .Where(progress => progress.ProfileId == profileId && progress.CourseId == courseId)
            .ToListAsync(cancellationToken);
        var completed = completedRows.Select(progress => progress.LessonId).ToHashSet();

        var entries = CourseProgressCalculator.Calculate(course, lessonsById, completed);
        var recommended = CourseProgressCalculator.RecommendNextLesson(entries);

        return new CourseProgressMap(course, entries, recommended);
    }

    /// <summary>
    /// True unless the lesson belongs to a known course and is currently
    /// locked. A lesson id that isn't part of any bundled course yet (content
    /// authoring is WP-021) never blocks — there is nothing to enforce against.
    /// </summary>
    public async Task<bool> IsLessonAvailableAsync(
        Guid profileId, string lessonId, CancellationToken cancellationToken = default)
    {
        var found = catalogue.FindLesson(lessonId);
        if (found is null)
        {
            return true;
        }

        var map = await GetCourseMapAsync(profileId, found.Value.CourseId, cancellationToken);
        var entry = map?.Entries.FirstOrDefault(e => e.LessonId == lessonId);
        return entry is null || entry.Availability != LessonAvailability.Locked;
    }

    /// <summary>
    /// Recalculates whether a profile has now met a lesson's completion rule
    /// and records it. Safe to call after every completed attempt; unknown
    /// lessons and lessons without an assessment are no-ops.
    /// </summary>
    public async Task EvaluateAndRecordCompletionAsync(
        Guid profileId, string lessonId, CancellationToken cancellationToken = default)
    {
        var found = catalogue.FindLesson(lessonId);
        if (found is null || found.Value.Lesson.Assessment is null)
        {
            return;
        }

        var (courseId, lesson) = found.Value;

        var alreadyCompleted = await db.LessonProgress
            .AnyAsync(progress => progress.ProfileId == profileId && progress.LessonId == lessonId, cancellationToken);
        if (alreadyCompleted)
        {
            return;
        }

        var attempts = await db.PracticeSessions
            .Where(session => session.ProfileId == profileId && session.LessonId == lessonId)
            .Include(session => session.Attempt)
            .Select(session => session.Attempt)
            .Where(attempt => attempt != null)
            .ToListAsync(cancellationToken);

        var rule = lesson.Assessment.CompletionRule;
        var successfulRuns = attempts.Count(attempt =>
            LessonCompletionEvaluator.IsSuccessfulRun(rule, DeserializeFacts(attempt!.ResultJson)));

        if (successfulRuns < rule.RequiredSuccessfulRuns)
        {
            return;
        }

        await EnsureEnrolledAsync(profileId, courseId, cancellationToken);

        db.LessonProgress.Add(new LessonProgressRecord
        {
            ProfileId = profileId,
            CourseId = courseId,
            LessonId = lessonId,
            CompletedAtUtc = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureEnrolledAsync(Guid profileId, string courseId, CancellationToken cancellationToken)
    {
        var alreadyEnrolled = await db.CourseEnrollments
            .AnyAsync(enrollment => enrollment.ProfileId == profileId && enrollment.CourseId == courseId, cancellationToken);
        if (alreadyEnrolled)
        {
            return;
        }

        db.CourseEnrollments.Add(new CourseEnrollment
        {
            ProfileId = profileId,
            CourseId = courseId,
            EnrolledAtUtc = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ScoringFactsSnapshot DeserializeFacts(string resultJson)
    {
        try
        {
            return JsonSerializer.Deserialize<ScoringFactsSnapshot>(resultJson, ContentJsonOptions.Default)
                ?? new ScoringFactsSnapshot();
        }
        catch (JsonException)
        {
            return new ScoringFactsSnapshot();
        }
    }
}
