namespace Nadiano.Core.Content.Manifests;

/// <summary>
/// Deserialized expected-events.json — the normalized scoring input the
/// browser matcher consumes (docs/CONTENT_MODEL.md §7). Generated from
/// MusicXML at content-build time (WP-013); MusicXML remains canonical for notation.
/// </summary>
public sealed class ExpectedEventDocument
{
    public required int SchemaVersion { get; init; }
    public required string TimeBase { get; init; }
    public required IReadOnlyList<TempoMapEntry> TempoMap { get; init; }
    public required IReadOnlyList<ExpectedEvent> Events { get; init; }
}

public sealed class TempoMapEntry
{
    public required double Beat { get; init; }
    public required int Bpm { get; init; }
}

/// <summary>A record so the MusicXML generator can build merged chord/tie events via `with` expressions.</summary>
public sealed record ExpectedEvent
{
    public required string Id { get; init; }
    public required int Measure { get; init; }
    public required double Beat { get; init; }
    public required double DurationBeats { get; init; }
    public required IReadOnlyList<int> Pitches { get; init; }
    public Hand? Hand { get; init; }
    public string? Voice { get; init; }
    public IReadOnlyList<int> Fingering { get; init; } = [];
    public ArticulationKind? Articulation { get; init; }
    public VelocityRange? VelocityTarget { get; init; }
}

public sealed class VelocityRange
{
    public required int Minimum { get; init; }
    public required int Maximum { get; init; }
}