namespace Nadiano.Core.Content.Validation;

public sealed class ContentValidationResult
{
    private readonly List<ValidationIssue> _issues = [];

    public IReadOnlyList<ValidationIssue> Issues => _issues;

    public bool IsValid => _issues.Count == 0;

    public void Add(string file, string? field, string reason) => _issues.Add(new ValidationIssue(file, field, reason));

    public void AddRange(IEnumerable<ValidationIssue> issues) => _issues.AddRange(issues);

    public void Merge(ContentValidationResult other) => _issues.AddRange(other.Issues);
}