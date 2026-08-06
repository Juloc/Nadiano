using Nadiano.Core.Beta;

namespace Nadiano.Core.Tests.Beta;

public class BetaLearningTests
{
    private static readonly GeneratedExerciseTemplate Template = new(
        "reading-c-position",
        GeneratedExerciseKind.Reading,
        "reading.c-position",
        60,
        67,
        4,
        4,
        [1, 2, 4],
        "treble");

    [Fact]
    public void Generate_WithSameSeed_IsDeterministic()
    {
        var first = SeededPracticeCardGenerator.Generate(Template, 12345);
        var second = SeededPracticeCardGenerator.Generate(Template, 12345);

        Assert.Equal(first.TemplateId, second.TemplateId);
        Assert.Equal(first.Seed, second.Seed);
        Assert.Equal(first.Events, second.Events);
    }

    [Fact]
    public void Generate_FillsEveryMeasureExactly()
    {
        var card = SeededPracticeCardGenerator.Generate(Template, 19);
        var expectedUnits = Template.BeatsPerMeasure * SeededPracticeCardGenerator.UnitsPerBeat;

        foreach (var measure in card.Events.GroupBy(item => item.Measure))
        {
            Assert.Equal(expectedUnits, measure.Sum(item => item.DurationUnits));
            Assert.All(measure, item => Assert.InRange(item.MidiNote, Template.MinimumMidiNote, Template.MaximumMidiNote));
        }
    }

    [Fact]
    public void RhythmEvaluator_MatchesNearestUnusedTap()
    {
        var result = RhythmEvaluator.Evaluate([0, 500, 1000], [15, 510, 1600], 80);

        Assert.Equal(2, result.MatchedCount);
        Assert.Equal(1, result.MissedCount);
        Assert.Equal(1, result.ExtraCount);
        Assert.False(result.Passed);
    }

    [Fact]
    public void ReviewScheduler_FailedAttemptReturnsTomorrow()
    {
        var completed = new DateTimeOffset(2026, 8, 6, 10, 0, 0, TimeSpan.Zero);

        var result = ReviewScheduler.Schedule(14, ReviewOutcome.Failed, completed);

        Assert.Equal(1, result.IntervalDays);
        Assert.Equal(completed.AddDays(1), result.DueAtUtc);
    }

    [Fact]
    public void AdaptiveAdvisor_RepeatedPitchErrorsSelectHandsSeparate()
    {
        var result = AdaptivePracticeAdvisor.Recommend(new PracticeEvidence(
            PitchErrors: 4,
            TimingScore: 0.9,
            RepeatedErrorSection: "measures-3-4",
            HandsSeparateAvailable: true,
            RhythmOnlyAvailable: true,
            CurrentTempoBpm: 100));

        Assert.Equal("hands-separate", result.PrimaryCode);
        Assert.Equal(85, result.SuggestedTempoBpm);
        Assert.Equal("measures-3-4", result.SuggestedSection);
    }

    [Fact]
    public void EarEvaluator_DoesNotRevealByCaseOrWhitespaceDifferences()
    {
        var prompt = new EarTrainingPrompt(
            "direction-1",
            EarTaskKind.Direction,
            [60, 64],
            [],
            "higher",
            2);

        var result = EarTrainingEvaluator.EvaluateAnswer(prompt, " Higher ");

        Assert.True(result.Correct);
    }
}
