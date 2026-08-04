namespace Nadiano.Core.Practice;

/// <summary>
/// One practice attempt in progress or completed (docs/TECHNICAL_ARCHITECTURE.md
/// §11 PracticeSessions table). Id is client-generated so completion can be
/// idempotent (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-017).
/// </summary>
public sealed class PracticeSessionRecord
{
    public required Guid Id { get; init; }
    public required Guid ProfileId { get; init; }
    public required string LessonId { get; init; }
    public required string ContentVersion { get; init; }
    public required string Mode { get; init; }
    public required DateTimeOffset StartedAtUtc { get; init; }

    public PracticeAttemptRecord? Attempt { get; set; }
}