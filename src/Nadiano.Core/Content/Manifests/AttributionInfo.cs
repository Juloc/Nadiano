namespace Nadiano.Core.Content.Manifests;

/// <summary>
/// Deserialized attribution.json. Every bundled item must declare whether
/// it is original Nadiano work, licensed material or a public-domain
/// composition (docs/AGENTS.md "Content"; docs/CONTENT_MODEL.md §14 license validation).
/// </summary>
public sealed class AttributionInfo
{
    public required string Source { get; init; }
    public required string License { get; init; }
    public string? Author { get; init; }
    public string? Notes { get; init; }
}