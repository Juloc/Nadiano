namespace Nadiano.Core.Progress;

/// <summary>
/// A learner's self-reported answer to one self-check question
/// (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-020 step 7; docs/TECHNICAL_ARCHITECTURE.md
/// §11 SkillEvidence table). This is evidence a learner supplied, not an
/// objective MIDI-measured result — docs/CONTENT_MODEL.md §6 forbids treating
/// a self-assessment skill as an objective pass.
/// </summary>
public sealed class SkillEvidenceRecord
{
    public required Guid Id { get; init; }
    public required Guid ProfileId { get; init; }
    public required string LessonId { get; init; }
    public required string SkillId { get; init; }
    public required bool SelfReportedSuccess { get; init; }
    public required DateTimeOffset RecordedAtUtc { get; init; }
}