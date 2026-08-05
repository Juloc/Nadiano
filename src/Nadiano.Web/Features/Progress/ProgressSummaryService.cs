using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;
using Nadiano.Core.Practice;
using Nadiano.Core.Progress;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.Features.Progress;

/// <summary>
/// Builds the read-only progress view from existing profile-scoped evidence.
/// It keeps completion coverage, objective MIDI observations and learner
/// self-checks separate so the page never implies more certainty than the
/// stored data supports.
/// </summary>
public sealed class ProgressSummaryService(
    NadianoDbContext db,
    ContentCatalogue catalogue,
    BundledContentRepository content,
    CourseProgressService courseProgress)
{
    private const int MinimumSamplesForAccuracy = 3;
    private const int MinimumSamplesForTrend = 6;
    private const int RecentItemLimit = 8;

    public async Task<ProgressSummary> BuildAsync(
        Guid profileId,
        string culture,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var sessions = await db.PracticeSessions
            .Where(session => session.ProfileId == profileId)
            .Include(session => session.Attempt)
            .ToListAsync(cancellationToken);

        var completedLessons = await db.LessonProgress
            .Where(progress => progress.ProfileId == profileId)
            .ToListAsync(cancellationToken);

        var selfChecks = await db.SkillEvidence
            .Where(evidence => evidence.ProfileId == profileId)
            .ToListAsync(cancellationToken);

        var completedAttempts = sessions
            .Where(session => session.Attempt is not null)
            .OrderByDescending(session => session.Attempt!.CompletedAtUtc)
            .ToArray();

        var completedLessonIds = completedLessons
            .Select(progress => progress.LessonId)
            .ToHashSet(StringComparer.Ordinal);

        var lessonEntries = GetLessonEntries();
        var lessonTitles = lessonEntries.ToDictionary(
            entry => entry.Lesson.Id,
            entry => LoadTitle(entry.CourseId, entry.Lesson.Id, culture),
            StringComparer.Ordinal);

        var competencies = BuildCompetencyProgress(lessonEntries, completedLessonIds);
        var observations = BuildCategoryObservations(completedAttempts, selfChecks);
        var reviewsDue = BuildReviewsDue(
            completedLessons,
            completedAttempts,
            lessonEntries,
            lessonTitles,
            nowUtc);

        var recommendation = await GetRecommendedLessonAsync(profileId, culture, cancellationToken);
        var recentPractice = completedAttempts
            .Take(RecentItemLimit)
            .Select(session => new RecentPracticeItem(
                session.LessonId,
                GetTitle(lessonTitles, session.LessonId),
                session.Attempt!.CompletedAtUtc,
                session.Attempt.NextActionCode))
            .ToArray();

        var recentCompletions = completedLessons
            .OrderByDescending(progress => progress.CompletedAtUtc)
            .Take(RecentItemLimit)
            .Select(progress => new CompletedLessonItem(
                progress.LessonId,
                GetTitle(lessonTitles, progress.LessonId),
                progress.CompletedAtUtc))
            .ToArray();

        return new ProgressSummary(
            completedLessonIds.Count,
            lessonEntries.Count,
            competencies,
            observations,
            recentPractice,
            recentCompletions,
            reviewsDue,
            recommendation,
            completedAttempts.FirstOrDefault()?.Attempt?.NextActionCode);
    }

    private IReadOnlyList<(string CourseId, LessonManifest Lesson)> GetLessonEntries()
    {
        var entries = new List<(string CourseId, LessonManifest Lesson)>();

        foreach (var courseId in catalogue.CourseIds)
        {
            if (!catalogue.TryGetCourse(courseId, out var course, out var lessonsById))
            {
                continue;
            }

            foreach (var lessonId in course.Stages.SelectMany(stage => stage.Items))
            {
                if (lessonsById.TryGetValue(lessonId, out var lesson))
                {
                    entries.Add((courseId, lesson));
                }
            }
        }

        return entries;
    }

    private IReadOnlyList<CompetencyAreaProgress> BuildCompetencyProgress(
        IReadOnlyList<(string CourseId, LessonManifest Lesson)> lessonEntries,
        IReadOnlySet<string> completedLessonIds)
    {
        var skillAreas = content.LoadSkillCatalogue().Skills
            .ToDictionary(skill => skill.Id, skill => skill.Competency, StringComparer.Ordinal);

        var lessonIdsByArea = new Dictionary<CompetencyArea, HashSet<string>>();
        var completedLessonIdsByArea = new Dictionary<CompetencyArea, HashSet<string>>();

        foreach (var (_, lesson) in lessonEntries)
        {
            var areas = lesson.Skills
                .Where(skillAreas.ContainsKey)
                .Select(skillId => skillAreas[skillId])
                .Distinct();

            foreach (var area in areas)
            {
                AddLesson(lessonIdsByArea, area, lesson.Id);
                if (completedLessonIds.Contains(lesson.Id))
                {
                    AddLesson(completedLessonIdsByArea, area, lesson.Id);
                }
            }
        }

        return lessonIdsByArea
            .OrderBy(pair => pair.Key)
            .Select(pair =>
            {
                var completedCount = completedLessonIdsByArea.TryGetValue(pair.Key, out var completed)
                    ? completed.Count
                    : 0;
                var percent = pair.Value.Count == 0
                    ? 0
                    : (int)Math.Round(completedCount * 100d / pair.Value.Count, MidpointRounding.AwayFromZero);

                return new CompetencyAreaProgress(
                    pair.Key.ToString().ToLowerInvariant(),
                    completedCount,
                    pair.Value.Count,
                    percent);
            })
            .ToArray();
    }

    private static IReadOnlyList<CategoryObservation> BuildCategoryObservations(
        IReadOnlyList<PracticeSessionRecord> completedAttempts,
        IReadOnlyList<SkillEvidenceRecord> selfChecks)
    {
        var chronologicalFacts = completedAttempts
            .OrderBy(session => session.Attempt!.CompletedAtUtc)
            .Select(session => DeserializeFacts(session.Attempt!.ResultJson))
            .ToArray();

        var pitchValues = chronologicalFacts
            .Where(facts => facts.Pitch is { TotalExpected: > 0 })
            .Select(facts =>
            {
                var pitch = facts.Pitch!;
                return pitch.CorrectCount / (double)(pitch.TotalExpected + Math.Max(0, pitch.AdditionCount));
            })
            .ToArray();

        var timingValues = chronologicalFacts
            .Where(facts => facts.Onset is { Deviations.Count: > 0 })
            .Select(facts => facts.Onset!.Deviations.Count(deviation => deviation.Band == TimingBand.OnTime)
                / (double)facts.Onset.Deviations.Count)
            .ToArray();

        var selfCheckValues = selfChecks
            .OrderBy(evidence => evidence.RecordedAtUtc)
            .Select(evidence => evidence.SelfReportedSuccess ? 1d : 0d)
            .ToArray();

        return
        [
            BuildObservation("pitch", pitchValues),
            BuildObservation("timing", timingValues),
            BuildObservation("self-check", selfCheckValues),
        ];
    }

    private static CategoryObservation BuildObservation(string categoryCode, IReadOnlyList<double> values)
    {
        int? accuracyPercent = values.Count >= MinimumSamplesForAccuracy
            ? (int)Math.Round(values.Average() * 100, MidpointRounding.AwayFromZero)
            : null;

        return new CategoryObservation(
            categoryCode,
            values.Count,
            accuracyPercent,
            CalculateTrend(values));
    }

    private static string? CalculateTrend(IReadOnlyList<double> values)
    {
        if (values.Count < MinimumSamplesForTrend)
        {
            return null;
        }

        var recent = values.Skip(values.Count - 3).Average();
        var previous = values.Skip(values.Count - 6).Take(3).Average();
        var difference = recent - previous;

        if (difference >= 0.05)
        {
            return "improving";
        }

        return difference <= -0.05 ? "needs-attention" : "stable";
    }

    private static IReadOnlyList<ReviewDueItem> BuildReviewsDue(
        IReadOnlyList<LessonProgressRecord> completedLessons,
        IReadOnlyList<PracticeSessionRecord> completedAttempts,
        IReadOnlyList<(string CourseId, LessonManifest Lesson)> lessonEntries,
        IReadOnlyDictionary<string, string> lessonTitles,
        DateTimeOffset nowUtc)
    {
        var lessonById = lessonEntries.ToDictionary(entry => entry.Lesson.Id, entry => entry.Lesson, StringComparer.Ordinal);
        var latestAttemptByLesson = completedAttempts
            .GroupBy(session => session.LessonId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Max(session => session.Attempt!.CompletedAtUtc),
                StringComparer.Ordinal);

        var due = new List<ReviewDueItem>();
        foreach (var progress in completedLessons)
        {
            if (!lessonById.TryGetValue(progress.LessonId, out var lesson))
            {
                continue;
            }

            var firstIntervalDays = lesson.Review?.InitialIntervalsDays.FirstOrDefault(days => days > 0) ?? 0;
            if (firstIntervalDays == 0)
            {
                continue;
            }

            var lastPracticeAt = latestAttemptByLesson.TryGetValue(progress.LessonId, out var latestAttempt)
                ? Max(progress.CompletedAtUtc, latestAttempt)
                : progress.CompletedAtUtc;
            var dueAt = lastPracticeAt.AddDays(firstIntervalDays);

            if (dueAt <= nowUtc)
            {
                due.Add(new ReviewDueItem(
                    progress.LessonId,
                    GetTitle(lessonTitles, progress.LessonId),
                    dueAt));
            }
        }

        return due.OrderBy(item => item.DueAtUtc).ToArray();
    }

    private async Task<LessonRecommendation?> GetRecommendedLessonAsync(
        Guid profileId,
        string culture,
        CancellationToken cancellationToken)
    {
        foreach (var courseId in catalogue.CourseIds)
        {
            var map = await courseProgress.GetCourseMapAsync(profileId, courseId, cancellationToken);
            if (map?.RecommendedNextLessonId is not { } lessonId)
            {
                continue;
            }

            return new LessonRecommendation(lessonId, LoadTitle(courseId, lessonId, culture));
        }

        return null;
    }

    private string LoadTitle(string courseId, string lessonId, string culture)
    {
        try
        {
            return content.LoadLocalizedText(courseId, lessonId, culture).Title;
        }
        catch (FileNotFoundException) when (!string.Equals(culture, "de", StringComparison.Ordinal))
        {
            return content.LoadLocalizedText(courseId, lessonId, "de").Title;
        }
    }

    private static string GetTitle(IReadOnlyDictionary<string, string> titles, string lessonId) =>
        titles.TryGetValue(lessonId, out var title) ? title : lessonId;

    private static void AddLesson(
        IDictionary<CompetencyArea, HashSet<string>> lessonsByArea,
        CompetencyArea area,
        string lessonId)
    {
        if (!lessonsByArea.TryGetValue(area, out var lessonIds))
        {
            lessonIds = new HashSet<string>(StringComparer.Ordinal);
            lessonsByArea[area] = lessonIds;
        }

        lessonIds.Add(lessonId);
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

    private static DateTimeOffset Max(DateTimeOffset left, DateTimeOffset right) => left >= right ? left : right;
}
