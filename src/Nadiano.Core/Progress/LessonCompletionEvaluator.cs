using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Progress;

/// <summary>
/// Decides whether a single practice attempt counts as a "successful run"
/// toward LessonManifest.Assessment.CompletionRule.RequiredSuccessfulRuns.
/// </summary>
public static class LessonCompletionEvaluator
{
    public static bool IsSuccessfulRun(CompletionRule rule, ScoringFactsSnapshot facts)
    {
        var pitchErrors = (facts.Pitch?.OmittedCount ?? 0) + (facts.Pitch?.AdditionCount ?? 0);
        if (pitchErrors > rule.MaximumPitchErrors)
        {
            return false;
        }

        return ComputeTimingScore(facts.Onset) >= rule.MinimumTimingScore;
    }

    // The onTime share of onset deviations. Bands are already the client's
    // calibrated timing tolerance, so re-deriving a score from raw deviationMs
    // here would duplicate (and could drift from) that calibration. No onset
    // data means nothing to penalize, so it scores as perfectly on time.
    private static double ComputeTimingScore(OnsetFactSnapshot? onset)
    {
        if (onset is null || onset.Deviations.Count == 0)
        {
            return 1.0;
        }

        var onTimeCount = onset.Deviations.Count(deviation => deviation.Band == TimingBand.OnTime);
        return (double)onTimeCount / onset.Deviations.Count;
    }
}
