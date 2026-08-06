namespace Nadiano.Core.Beta;

public enum ReviewOutcome
{
    Excellent,
    Good,
    NeedsWork,
    Failed,
}

public sealed record ReviewScheduleDecision(
    DateTimeOffset DueAtUtc,
    int IntervalDays,
    string ReasonCode);

public static class ReviewScheduler
{
    public static ReviewScheduleDecision Schedule(
        int previousIntervalDays,
        ReviewOutcome outcome,
        DateTimeOffset completedAtUtc,
        IReadOnlyList<int>? initialIntervalsDays = null)
    {
        var initial = (initialIntervalsDays ?? [1, 3, 7, 14])
            .Where(value => value > 0)
            .Distinct()
            .OrderBy(value => value)
            .ToArray();

        if (initial.Length == 0)
        {
            initial = [1, 3, 7, 14];
        }

        var interval = outcome switch
        {
            ReviewOutcome.Failed => 1,
            ReviewOutcome.NeedsWork => Math.Max(1, previousIntervalDays / 2),
            ReviewOutcome.Good => NextInterval(previousIntervalDays, initial, 2),
            ReviewOutcome.Excellent => NextInterval(previousIntervalDays, initial, 3),
            _ => 1,
        };

        var reason = outcome switch
        {
            ReviewOutcome.Failed => "review-after-failed-attempt",
            ReviewOutcome.NeedsWork => "review-after-weak-category",
            ReviewOutcome.Good => "review-after-good-attempt",
            ReviewOutcome.Excellent => "review-after-strong-attempt",
            _ => "review-due",
        };

        return new ReviewScheduleDecision(completedAtUtc.AddDays(interval), interval, reason);
    }

    private static int NextInterval(int previousIntervalDays, IReadOnlyList<int> initial, int multiplier)
    {
        if (previousIntervalDays <= 0)
        {
            return initial[0];
        }

        var nextConfigured = initial.FirstOrDefault(value => value > previousIntervalDays);
        return nextConfigured > 0
            ? nextConfigured
            : Math.Min(180, checked(previousIntervalDays * multiplier));
    }
}

public sealed record PracticeEvidence(
    int PitchErrors,
    double TimingScore,
    string? RepeatedErrorSection,
    bool HandsSeparateAvailable,
    bool RhythmOnlyAvailable,
    int CurrentTempoBpm);

public sealed record PracticeRecommendation(
    string PrimaryCode,
    int SuggestedTempoBpm,
    string? SuggestedSection,
    IReadOnlyList<string> Alternatives);

public static class AdaptivePracticeAdvisor
{
    public static PracticeRecommendation Recommend(PracticeEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var slowerTempo = Math.Max(30, (int)Math.Round(evidence.CurrentTempoBpm * 0.85, MidpointRounding.AwayFromZero));
        var alternatives = new List<string>();

        if (evidence.PitchErrors >= 3 && evidence.HandsSeparateAvailable)
        {
            if (evidence.RepeatedErrorSection is not null)
            {
                alternatives.Add("smaller-section");
            }
            alternatives.Add("repeat-slower");
            return new PracticeRecommendation("hands-separate", slowerTempo, evidence.RepeatedErrorSection, alternatives);
        }

        if (evidence.TimingScore < 0.65 && evidence.RhythmOnlyAvailable)
        {
            alternatives.Add("repeat-slower");
            alternatives.Add("listen-and-copy");
            return new PracticeRecommendation("rhythm-only", slowerTempo, evidence.RepeatedErrorSection, alternatives);
        }

        if (evidence.RepeatedErrorSection is not null)
        {
            alternatives.Add("repeat-slower");
            return new PracticeRecommendation("smaller-section", slowerTempo, evidence.RepeatedErrorSection, alternatives);
        }

        if (evidence.PitchErrors > 0 || evidence.TimingScore < 0.8)
        {
            alternatives.Add("tempo-ladder");
            return new PracticeRecommendation("repeat-slower", slowerTempo, null, alternatives);
        }

        return new PracticeRecommendation(
            "tempo-ladder",
            Math.Min(240, evidence.CurrentTempoBpm + 5),
            null,
            ["continue-course"]);
    }
}
