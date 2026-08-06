using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Beta;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.Features.Beta;

public sealed record RecordBetaEvidenceRequest(
    string ActivityId,
    string ActivityKind,
    int? Seed,
    string SkillId,
    JsonElement Expected,
    JsonElement Response,
    JsonElement Result,
    string Outcome);

public sealed record BetaSessionPlanItem(string ActivityId, string SkillId, string ReasonCode, int? Seed);

public sealed class BetaLearningService(NadianoDbContext db)
{
    private static readonly GeneratedExerciseTemplate ReadingTemplate = new(
        "beta-reading",
        GeneratedExerciseKind.Reading,
        "reading.generated",
        48,
        72,
        4,
        4,
        [1, 2, 4],
        "mixed");

    private static readonly GeneratedExerciseTemplate RhythmTemplate = new(
        "beta-rhythm",
        GeneratedExerciseKind.Rhythm,
        "rhythm.generated",
        60,
        60,
        4,
        4,
        [1, 2, 4],
        "percussion");

    public BetaCurriculum Curriculum { get; } = BetaCurriculumCatalogue.Create();

    public GeneratedPracticeCard GenerateCard(GeneratedExerciseKind kind, int seed) =>
        SeededPracticeCardGenerator.Generate(kind == GeneratedExerciseKind.Rhythm ? RhythmTemplate : ReadingTemplate, seed);

    public async Task RecordAsync(Guid profileId, RecordBetaEvidenceRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        var now = DateTimeOffset.UtcNow;
        db.LearningEvidence.Add(new LearningEvidenceRecord
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            ActivityId = request.ActivityId,
            ActivityKind = request.ActivityKind,
            Seed = request.Seed,
            ExpectedJson = request.Expected.GetRawText(),
            ResponseJson = request.Response.GetRawText(),
            ResultJson = request.Result.GetRawText(),
            RecordedAtUtc = now,
        });

        var existing = await db.ReviewQueue.SingleOrDefaultAsync(
            item => item.ProfileId == profileId && item.SkillId == request.SkillId && item.SourceId == request.ActivityId,
            cancellationToken);
        if (!Enum.TryParse<ReviewOutcome>(request.Outcome, ignoreCase: true, out var outcome))
        {
            throw new ArgumentException("Invalid review outcome.", nameof(request));
        }

        var decision = ReviewScheduler.Schedule(existing?.IntervalDays ?? 0, outcome, now);

        if (existing is null)
        {
            db.ReviewQueue.Add(new ReviewQueueItem
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                SkillId = request.SkillId,
                SourceId = request.ActivityId,
                DueAtUtc = decision.DueAtUtc,
                IntervalDays = decision.IntervalDays,
                ReasonCode = decision.ReasonCode,
                UpdatedAtUtc = now,
            });
        }
        else
        {
            existing.DueAtUtc = decision.DueAtUtc;
            existing.IntervalDays = decision.IntervalDays;
            existing.ReasonCode = decision.ReasonCode;
            existing.UpdatedAtUtc = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewQueueItem>> DueAsync(Guid profileId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var items = await db.ReviewQueue
            .AsNoTracking()
            .Where(item => item.ProfileId == profileId)
            .ToListAsync(cancellationToken);

        return items
            .Where(item => item.DueAtUtc <= now)
            .OrderBy(item => item.DueAtUtc)
            .ThenBy(item => item.SkillId, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<IReadOnlyList<BetaSessionPlanItem>> BuildSessionPlanAsync(
        Guid profileId,
        DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var due = await DueAsync(profileId, DateTimeOffset.UtcNow, cancellationToken);
        var plan = due.Take(3)
            .Select(item => new BetaSessionPlanItem(item.SourceId, item.SkillId, item.ReasonCode, null))
            .ToList();

        var dateSeed = localDate.Year * 10_000 + localDate.Month * 100 + localDate.Day;
        if (plan.Count < 4)
        {
            plan.Add(new BetaSessionPlanItem("beta-reading", "reading.generated", "balanced-reading", dateSeed));
        }
        if (plan.Count < 5)
        {
            plan.Add(new BetaSessionPlanItem("beta-rhythm", "rhythm.generated", "balanced-rhythm", dateSeed + 1));
        }
        if (plan.All(item => !item.SkillId.StartsWith("ear", StringComparison.Ordinal)))
        {
            plan.Add(new BetaSessionPlanItem("beta-ear-direction", "ear.direction", "balanced-ear", dateSeed + 2));
        }

        return plan;
    }

    private static void ValidateRequest(RecordBetaEvidenceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ActivityId) || request.ActivityId.Length > 200 ||
            string.IsNullOrWhiteSpace(request.ActivityKind) || request.ActivityKind.Length > 50 ||
            string.IsNullOrWhiteSpace(request.SkillId) || request.SkillId.Length > 200)
        {
            throw new ArgumentException("Invalid beta evidence identifiers.", nameof(request));
        }
    }
}
