namespace Nadiano.Core.Content.Validation;

/// <summary>One reported problem: file, field and reason (docs/CONTENT_MODEL.md §14 acceptance criteria).</summary>
public sealed record ValidationIssue(string File, string? Field, string Reason)
{
    public override string ToString() =>
        Field is null ? $"{File}: {Reason}" : $"{File} [{Field}]: {Reason}";
}