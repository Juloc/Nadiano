namespace Nadiano.Core.Practice;

/// <summary>
/// The completed result of a PracticeSessionRecord (docs/TECHNICAL_ARCHITECTURE.md
/// §11 PracticeAttempts table). ResultJson is a versioned evidence payload —
/// the ScoringFacts + next-action recommendation the client computed
/// (docs/TECHNICAL_ARCHITECTURE.md §8: "Store enough normalized evidence to
/// explain the result").
/// </summary>
public sealed class PracticeAttemptRecord
{
    public required Guid Id { get; init; }
    public required Guid SessionId { get; init; }
    public required DateTimeOffset CompletedAtUtc { get; init; }
    public required int ResultSchemaVersion { get; init; }
    public required string ResultJson { get; init; }
    public required string NextActionCode { get; init; }
}