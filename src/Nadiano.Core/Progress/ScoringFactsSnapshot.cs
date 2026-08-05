using System.Text.Json.Serialization;

namespace Nadiano.Core.Progress;

/// <summary>
/// Mirrors the subset of wwwroot/js/scoring/facts.ts ScoringFacts needed to
/// evaluate a LessonManifest.Assessment.CompletionRule server-side. Only the
/// categories the rule inspects (pitch, onset) are represented.
/// </summary>
public sealed class ScoringFactsSnapshot
{
    public PitchFactSnapshot? Pitch { get; init; }
    public OnsetFactSnapshot? Onset { get; init; }
}

public sealed class PitchFactSnapshot
{
    public int TotalExpected { get; init; }
    public int CorrectCount { get; init; }
    public int OmittedCount { get; init; }
    public int AdditionCount { get; init; }
}

public sealed class OnsetFactSnapshot
{
    public IReadOnlyList<OnsetDeviationSnapshot> Deviations { get; init; } = [];
}

public sealed class OnsetDeviationSnapshot
{
    public double DeviationMs { get; init; }
    public TimingBand Band { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<TimingBand>))]
public enum TimingBand
{
    [JsonStringEnumMemberName("onTime")]
    OnTime,

    [JsonStringEnumMemberName("early")]
    Early,

    [JsonStringEnumMemberName("late")]
    Late,
}
