using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Content;

public sealed class ExpectedEventGenerationResult
{
    public ExpectedEventDocument? Document { get; init; }

    /// <summary>Notation constructs this generator does not support, reported rather than guessed (docs/CONTENT_MODEL.md §7).</summary>
    public IReadOnlyList<string> UnsupportedConstructs { get; init; } = [];

    public bool Success => Document is not null && UnsupportedConstructs.Count == 0;
}