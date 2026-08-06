namespace Nadiano.Core.Beta;

public sealed class LearningEvidenceRecord
{
    public required Guid Id { get; init; }
    public required Guid ProfileId { get; init; }
    public required string ActivityId { get; init; }
    public required string ActivityKind { get; init; }
    public int? Seed { get; init; }
    public required string ExpectedJson { get; init; }
    public required string ResponseJson { get; init; }
    public required string ResultJson { get; init; }
    public required DateTimeOffset RecordedAtUtc { get; init; }
}

public sealed class ReviewQueueItem
{
    public required Guid Id { get; init; }
    public required Guid ProfileId { get; init; }
    public required string SkillId { get; init; }
    public required string SourceId { get; init; }
    public required DateTimeOffset DueAtUtc { get; set; }
    public required int IntervalDays { get; set; }
    public required string ReasonCode { get; set; }
    public required DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class PrivateLibraryItem
{
    public required Guid Id { get; init; }
    public required Guid ProfileId { get; init; }
    public required string DisplayTitle { get; set; }
    public required string SourceFileName { get; init; }
    public required string StoredDirectoryName { get; init; }
    public required string OriginalSha256 { get; init; }
    public required long ContentLength { get; init; }
    public required string ValidationState { get; set; }
    public required string WarningJson { get; set; }
    public required string MetadataJson { get; set; }
    public required int Version { get; set; }
    public required DateTimeOffset ImportedAtUtc { get; init; }
}
