namespace Nadiano.Core.Profiles;

/// <summary>
/// A local household learner. No internet account is required for 1.0
/// (see docs/PRODUCT_CONCEPT.md §5).
/// </summary>
public sealed class LearnerProfile
{
    public required Guid Id { get; init; }

    public required string Name { get; set; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
}