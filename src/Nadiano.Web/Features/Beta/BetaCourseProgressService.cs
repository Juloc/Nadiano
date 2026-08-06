using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Beta;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.Features.Beta;

public sealed record BetaCourseMapItem(BetaLessonDescriptor Lesson, string Status, string? LockedBy);

public sealed record BetaLessonCompletionResult(bool Success, string Code);

public sealed class BetaCourseProgressService(
    NadianoDbContext db,
    BetaLearningService learning)
{
    public async Task<IReadOnlyList<BetaCourseMapItem>> GetMapAsync(Guid profileId, CancellationToken cancellationToken)
    {
        var completedIds = await db.LearningEvidence
            .AsNoTracking()
            .Where(item => item.ProfileId == profileId &&
                (item.ActivityKind == "beta-lesson" || item.ActivityKind == "beta-stage-check"))
            .Select(item => item.ActivityId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var completed = completedIds.ToHashSet(StringComparer.Ordinal);
        var result = new List<BetaCourseMapItem>(learning.Curriculum.Lessons.Count);
        string? previousId = null;

        foreach (var lesson in learning.Curriculum.Lessons.OrderBy(item => item.Order))
        {
            var status = completed.Contains(lesson.Id)
                ? "completed"
                : previousId is null || completed.Contains(previousId)
                    ? "available"
                    : "locked";
            result.Add(new BetaCourseMapItem(lesson, status, status == "locked" ? previousId : null));
            previousId = lesson.Id;
        }

        return result;
    }

    public async Task<BetaLessonCompletionResult> CompleteAsync(
        Guid profileId,
        string lessonId,
        CancellationToken cancellationToken)
    {
        var map = await GetMapAsync(profileId, cancellationToken);
        var item = map.SingleOrDefault(candidate => candidate.Lesson.Id == lessonId);
        if (item is null)
        {
            return new(false, "not-found");
        }
        if (item.Status == "locked")
        {
            return new(false, "locked");
        }
        if (item.Status == "completed")
        {
            return new(true, "already-completed");
        }

        var isStageCheck = item.Lesson.ActivityKind == "stage-check";
        if (isStageCheck && !await HasStageEvidenceAsync(profileId, cancellationToken))
        {
            return new(false, "stage-evidence-required");
        }

        var now = DateTimeOffset.UtcNow;
        db.LearningEvidence.Add(new LearningEvidenceRecord
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            ActivityId = item.Lesson.Id,
            ActivityKind = isStageCheck ? "beta-stage-check" : "beta-lesson",
            Seed = null,
            ExpectedJson = "{}",
            ResponseJson = "{}",
            ResultJson = JsonSerializer.Serialize(new { completed = true, completedAtUtc = now }),
            RecordedAtUtc = now,
        });
        await db.SaveChangesAsync(cancellationToken);
        return new(true, "completed");
    }

    private async Task<bool> HasStageEvidenceAsync(Guid profileId, CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-30);
        var kinds = await db.LearningEvidence
            .AsNoTracking()
            .Where(item => item.ProfileId == profileId && item.RecordedAtUtc >= cutoff)
            .Select(item => item.ActivityKind)
            .Distinct()
            .ToListAsync(cancellationToken);

        return kinds.Contains("reading-card", StringComparer.Ordinal) &&
            kinds.Contains("rhythm-card", StringComparer.Ordinal) &&
            kinds.Contains("ear-direction", StringComparer.Ordinal);
    }
}