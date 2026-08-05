using Nadiano.Core.Content.Manifests;
using Nadiano.Core.Progress;

namespace Nadiano.Core.Tests.Progress;

public class LessonCompletionEvaluatorTests
{
    private static readonly CompletionRule Rule = new()
    {
        RequiredSuccessfulRuns = 2,
        MaximumPitchErrors = 0,
        MinimumTimingScore = 0.75,
    };

    [Fact]
    public void IsSuccessfulRun_WithNoErrorsAndAllOnTime_ReturnsTrue()
    {
        var facts = new ScoringFactsSnapshot
        {
            Pitch = new PitchFactSnapshot { OmittedCount = 0, AdditionCount = 0 },
            Onset = new OnsetFactSnapshot
            {
                Deviations = [new OnsetDeviationSnapshot { Band = TimingBand.OnTime }],
            },
        };

        Assert.True(LessonCompletionEvaluator.IsSuccessfulRun(Rule, facts));
    }

    [Fact]
    public void IsSuccessfulRun_WithMorePitchErrorsThanAllowed_ReturnsFalse()
    {
        var facts = new ScoringFactsSnapshot
        {
            Pitch = new PitchFactSnapshot { OmittedCount = 1, AdditionCount = 0 },
        };

        Assert.False(LessonCompletionEvaluator.IsSuccessfulRun(Rule, facts));
    }

    [Fact]
    public void IsSuccessfulRun_WithTimingScoreBelowMinimum_ReturnsFalse()
    {
        var facts = new ScoringFactsSnapshot
        {
            Onset = new OnsetFactSnapshot
            {
                Deviations =
                [
                    new OnsetDeviationSnapshot { Band = TimingBand.OnTime },
                    new OnsetDeviationSnapshot { Band = TimingBand.Late },
                    new OnsetDeviationSnapshot { Band = TimingBand.Late },
                    new OnsetDeviationSnapshot { Band = TimingBand.Late },
                ],
            },
        };

        // 1 of 4 on time = 0.25, below the 0.75 minimum.
        Assert.False(LessonCompletionEvaluator.IsSuccessfulRun(Rule, facts));
    }

    [Fact]
    public void IsSuccessfulRun_WithNoOnsetDataAtAll_TreatsTimingAsPerfect()
    {
        var facts = new ScoringFactsSnapshot { Pitch = new PitchFactSnapshot() };

        Assert.True(LessonCompletionEvaluator.IsSuccessfulRun(Rule, facts));
    }
}