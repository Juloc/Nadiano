namespace Nadiano.Core.Content.Manifests;

/// <summary>
/// Deserialized technique demonstration metadata (docs/CONTENT_MODEL.md §9).
/// For this build, "views" point at original illustrations rather than
/// filmed video — see docs/decisions and the alpha release notes for why.
/// </summary>
public sealed class TechniqueMediaMetadata
{
    public required string Id { get; init; }
    public required IReadOnlyList<TechniqueMediaView> Views { get; init; }
    public bool HasAudioDescription { get; init; }
    public bool Loop { get; init; }
}

public sealed class TechniqueMediaView
{
    public required string Kind { get; init; }
    public required string Path { get; init; }
    public string? Poster { get; init; }
}