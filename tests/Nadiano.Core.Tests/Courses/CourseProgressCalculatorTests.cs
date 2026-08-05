using Nadiano.Core.Content.Manifests;
using Nadiano.Core.Courses;

namespace Nadiano.Core.Tests.Courses;

public class CourseProgressCalculatorTests
{
    private static readonly CourseManifest Course = new()
    {
        SchemaVersion = 1,
        Id = "fixture-course",
        Version = "0.1.0",
        DefaultLanguage = "de",
        SupportedLanguages = ["de", "id"],
        Stages =
        [
            new CourseStage
            {
                Id = "F0",
                TitleKey = "course.stage.f0",
                Items = ["lesson-a", "lesson-b", "lesson-c"],
            },
        ],
    };

    private static readonly IReadOnlyDictionary<string, LessonManifest> Lessons = new Dictionary<string, LessonManifest>
    {
        ["lesson-a"] = CreateLesson("lesson-a", prerequisites: []),
        ["lesson-b"] = CreateLesson("lesson-b", prerequisites: ["lesson-a"]),
        ["lesson-c"] = CreateLesson("lesson-c", prerequisites: ["lesson-b"]),
    };

    private static LessonManifest CreateLesson(string id, IReadOnlyList<string> prerequisites) => new()
    {
        SchemaVersion = 1,
        Id = id,
        Kind = LessonKind.TechniqueDrill,
        Stage = "F0",
        Order = 10,
        EstimatedMinutes = 5,
        Prerequisites = prerequisites,
        Localization = new LocalizationConfig { Directory = "i18n" },
    };

    [Fact]
    public void Calculate_WithNoCompletions_OnlyTheFirstLessonIsAvailable()
    {
        var entries = CourseProgressCalculator.Calculate(Course, Lessons, new HashSet<string>());

        Assert.Equal(LessonAvailability.Available, entries.Single(e => e.LessonId == "lesson-a").Availability);
        Assert.Equal(LessonAvailability.Locked, entries.Single(e => e.LessonId == "lesson-b").Availability);
        Assert.Equal(LessonAvailability.Locked, entries.Single(e => e.LessonId == "lesson-c").Availability);
        Assert.Equal(["lesson-a"], entries.Single(e => e.LessonId == "lesson-b").MissingPrerequisites);
    }

    [Fact]
    public void Calculate_AfterCompletingTheFirstLesson_UnlocksOnlyTheNextOne()
    {
        var entries = CourseProgressCalculator.Calculate(Course, Lessons, new HashSet<string> { "lesson-a" });

        Assert.Equal(LessonAvailability.Completed, entries.Single(e => e.LessonId == "lesson-a").Availability);
        Assert.Equal(LessonAvailability.Available, entries.Single(e => e.LessonId == "lesson-b").Availability);
        Assert.Equal(LessonAvailability.Locked, entries.Single(e => e.LessonId == "lesson-c").Availability);
    }

    [Fact]
    public void Calculate_AfterCompletingAllPrerequisites_UnlocksTheFinalLesson()
    {
        var entries = CourseProgressCalculator.Calculate(Course, Lessons, new HashSet<string> { "lesson-a", "lesson-b" });

        Assert.Equal(LessonAvailability.Available, entries.Single(e => e.LessonId == "lesson-c").Availability);
        Assert.Empty(entries.Single(e => e.LessonId == "lesson-c").MissingPrerequisites);
    }

    [Theory]
    [InlineData(new string[] { }, "lesson-a")]
    [InlineData(new[] { "lesson-a" }, "lesson-b")]
    [InlineData(new[] { "lesson-a", "lesson-b" }, "lesson-c")]
    public void RecommendNextLesson_ReturnsTheFirstAvailableLessonInCourseOrder(string[] completed, string expected)
    {
        var entries = CourseProgressCalculator.Calculate(Course, Lessons, completed.ToHashSet());

        Assert.Equal(expected, CourseProgressCalculator.RecommendNextLesson(entries));
    }

    [Fact]
    public void RecommendNextLesson_ReturnsNull_WhenEveryLessonIsCompleted()
    {
        var entries = CourseProgressCalculator.Calculate(Course, Lessons, new HashSet<string> { "lesson-a", "lesson-b", "lesson-c" });

        Assert.Null(CourseProgressCalculator.RecommendNextLesson(entries));
    }
}