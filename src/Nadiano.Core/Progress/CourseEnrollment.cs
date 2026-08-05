namespace Nadiano.Core.Progress;

/// <summary>A learner has started a course (docs/TECHNICAL_ARCHITECTURE.md §11 CourseEnrollments table).</summary>
public sealed class CourseEnrollment
{
    public required Guid ProfileId { get; init; }
    public required string CourseId { get; init; }
    public required DateTimeOffset EnrolledAtUtc { get; init; }
}