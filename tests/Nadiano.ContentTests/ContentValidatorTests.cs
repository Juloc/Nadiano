using Nadiano.Core.Content;
using Nadiano.Core.Content.Validation;

namespace Nadiano.ContentTests;

public class ContentValidatorTests
{
    private static string FixturePath(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    [Fact]
    public void ValidateAll_ReportsNoIssues_ForValidContent()
    {
        var repository = new BundledContentRepository(FixturePath("valid-content"));
        var validator = new ContentValidator(repository);

        var result = validator.ValidateAll();

        Assert.True(result.IsValid, string.Join("\n", result.Issues));
    }

    [Fact]
    public void ValidateAll_ReportsFileFieldAndReason_ForInvalidContent()
    {
        var repository = new BundledContentRepository(FixturePath("invalid-content"));
        var validator = new ContentValidator(repository);

        var result = validator.ValidateAll();

        Assert.False(result.IsValid);
        Assert.All(result.Issues, issue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(issue.File));
            Assert.False(string.IsNullOrWhiteSpace(issue.Reason));
        });
    }

    [Fact]
    public void ValidateAll_DetectsDuplicateSkillIdAndUnknownRelatedSkill()
    {
        var repository = new BundledContentRepository(FixturePath("invalid-content"));
        var result = new ContentValidator(repository).ValidateAll();

        Assert.Contains(result.Issues, i => i.Reason.Contains("Duplicate skill id"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("unknown related skill"));
    }

    [Fact]
    public void ValidateAll_DetectsCourseLevelProblems()
    {
        var repository = new BundledContentRepository(FixturePath("invalid-content"));
        var result = new ContentValidator(repository).ValidateAll();

        Assert.Contains(result.Issues, i => i.Reason.Contains("DefaultLanguage must be included"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("unknown lesson id 'ghost-lesson'"));
    }

    [Fact]
    public void ValidateAll_DetectsLessonLevelProblems()
    {
        var repository = new BundledContentRepository(FixturePath("invalid-content"));
        var result = new ContentValidator(repository).ValidateAll();

        Assert.Contains(result.Issues, i => i.Reason.Contains("Order must not be negative"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("EstimatedMinutes must be positive"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("Unknown skill id 'totally-unknown-skill'"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("cannot list itself as a prerequisite"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("Unknown self-check skill id"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("Missing attribution.json"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("Missing localization file"));
        Assert.Contains(result.Issues, i => i.Reason.Contains("Referenced technique media file does not exist"));
    }

    [Fact]
    public void ValidateAll_DoesNotThrow_WhenContentRootIsEmpty()
    {
        var emptyDirectory = Path.Combine(Path.GetTempPath(), $"nadiano-empty-content-{Guid.NewGuid():N}");
        Directory.CreateDirectory(emptyDirectory);

        try
        {
            var repository = new BundledContentRepository(emptyDirectory);
            var result = new ContentValidator(repository).ValidateAll();

            Assert.False(result.IsValid);
            Assert.Contains(result.Issues, i => i.File.EndsWith("skills.json"));
        }
        finally
        {
            Directory.Delete(emptyDirectory, recursive: true);
        }
    }
}