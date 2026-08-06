namespace Nadiano.Core.Beta;

public sealed record RhythmEvaluation(
    int ExpectedCount,
    int MatchedCount,
    int MissedCount,
    int ExtraCount,
    double AverageAbsoluteDeviationMs,
    bool Passed);

public static class RhythmEvaluator
{
    public static RhythmEvaluation Evaluate(
        IReadOnlyList<double> expectedOnsetsMs,
        IReadOnlyList<double> performedOnsetsMs,
        double toleranceMs)
    {
        ArgumentNullException.ThrowIfNull(expectedOnsetsMs);
        ArgumentNullException.ThrowIfNull(performedOnsetsMs);

        if (toleranceMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(toleranceMs));
        }

        var unused = performedOnsetsMs
            .Select((value, index) => new PerformedOnset(index, value))
            .ToList();
        var deviations = new List<double>();

        foreach (var expected in expectedOnsetsMs)
        {
            var match = unused
                .Select(item => new MatchCandidate(item, Math.Abs(item.Value - expected)))
                .Where(candidate => candidate.Deviation <= toleranceMs)
                .OrderBy(candidate => candidate.Deviation)
                .ThenBy(candidate => candidate.Item.Index)
                .FirstOrDefault();

            if (match is null)
            {
                continue;
            }

            deviations.Add(match.Deviation);
            unused.Remove(match.Item);
        }

        var matched = deviations.Count;
        var missed = expectedOnsetsMs.Count - matched;
        var extra = unused.Count;
        var averageDeviation = matched == 0 ? 0 : deviations.Average();
        var passed = missed == 0 && extra <= 1 && averageDeviation <= toleranceMs * 0.6;

        return new RhythmEvaluation(
            expectedOnsetsMs.Count,
            matched,
            missed,
            extra,
            averageDeviation,
            passed);
    }

    private sealed record PerformedOnset(int Index, double Value);
    private sealed record MatchCandidate(PerformedOnset Item, double Deviation);
}

public enum EarTaskKind
{
    Direction,
    SameDifferent,
    Imitation,
    RhythmEcho,
}

public sealed record EarTrainingPrompt(
    string Id,
    EarTaskKind Kind,
    IReadOnlyList<int> ReferenceMidiNotes,
    IReadOnlyList<int> ComparisonMidiNotes,
    string ExpectedAnswer,
    int ReplayLimit);

public sealed record EarTrainingEvaluation(bool Correct, string ResultCode);

public static class EarTrainingEvaluator
{
    public static EarTrainingEvaluation EvaluateAnswer(EarTrainingPrompt prompt, string answer)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        var normalizedExpected = Normalize(prompt.ExpectedAnswer);
        var normalizedAnswer = Normalize(answer);
        var correct = normalizedExpected.Length > 0 && normalizedExpected == normalizedAnswer;

        return new EarTrainingEvaluation(correct, correct ? "correct" : "try-again");
    }

    public static EarTrainingEvaluation EvaluateImitation(
        EarTrainingPrompt prompt,
        IReadOnlyList<int> performedMidiNotes)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        ArgumentNullException.ThrowIfNull(performedMidiNotes);

        var correct = prompt.ReferenceMidiNotes.SequenceEqual(performedMidiNotes);
        return new EarTrainingEvaluation(correct, correct ? "correct" : "sequence-differs");
    }

    private static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant();
}
