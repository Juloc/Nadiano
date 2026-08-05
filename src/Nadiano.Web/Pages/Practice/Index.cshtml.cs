using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Practice;

public class IndexModel(
    ContentCatalogue catalogue,
    CourseProgressService progress,
    BundledContentRepository content,
    CurrentProfileAccessor profiles) : PageModel
{
    public bool HasLesson { get; private set; }
    public string LessonId { get; private set; } = string.Empty;
    public string LessonTitle { get; private set; } = string.Empty;
    public string ContentVersion { get; private set; } = string.Empty;
    public string ScoreUrl { get; private set; } = string.Empty;
    public string ExpectedEventsUrl { get; private set; } = string.Empty;
    public int TargetTempo { get; private set; }
    public int CountInMeasures { get; private set; }
    public string DefaultMode { get; private set; } = "wait";
    public IReadOnlyList<string> SupportedModes { get; private set; } = [];
    public string AssessmentCategoriesJson { get; private set; } = "[]";

    public async Task<IActionResult> OnGetAsync(string? lessonId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(lessonId))
        {
            return Page();
        }

        var found = catalogue.FindLesson(lessonId);
        if (found is null)
        {
            return NotFound();
        }

        var (courseId, lesson) = found.Value;
        if (lesson.Notation is null || lesson.Practice is null || lesson.Assessment is null)
        {
            return RedirectToPage("/Learn/Lesson", new { lessonId });
        }

        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        if (!await progress.IsLessonAvailableAsync(profileId, lessonId, cancellationToken))
        {
            return RedirectToPage("/Learn/Index");
        }

        catalogue.TryGetCourse(courseId, out var course, out _);
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        HasLesson = true;
        LessonId = lessonId;
        LessonTitle = content.LoadLocalizedText(courseId, lessonId, culture).Title;
        ContentVersion = course.Version;
        ScoreUrl = ContentUrl(lessonId, lesson.Notation.Path);
        ExpectedEventsUrl = ContentUrl(lessonId, "expected-events.json");
        TargetTempo = lesson.Practice.TargetTempo;
        CountInMeasures = lesson.Practice.CountInMeasures;
        DefaultMode = ModeCode(lesson.Practice.DefaultMode);
        SupportedModes = lesson.Practice.SupportedModes
            .Select(ModeCode)
            .Where(mode => mode is "wait" or "loop" or "hands-separate" or "performance")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        AssessmentCategoriesJson = System.Text.Json.JsonSerializer.Serialize(
            lesson.Assessment.Categories.Select(CategoryCode).ToArray());

        return Page();
    }

    public static string ModeResourceKey(string mode) => mode switch
    {
        "wait" => "Practice.Workspace.ModeWait",
        "loop" => "Practice.Workspace.ModeLoop",
        "hands-separate" => "Practice.Workspace.ModeHandsSeparate",
        "performance" => "Practice.Workspace.ModePerformance",
        _ => "Practice.Workspace.ModeWait",
    };

    private static string ContentUrl(string lessonId, string relativePath) =>
        $"/api/content/lessons/{Uri.EscapeDataString(lessonId)}/files/{relativePath}";

    private static string ModeCode(PracticeMode mode) => mode switch
    {
        PracticeMode.Wait => "wait",
        PracticeMode.Loop => "loop",
        PracticeMode.HandsSeparate => "hands-separate",
        PracticeMode.Performance => "performance",
        _ => "wait",
    };

    private static string CategoryCode(AssessmentCategory category) => category.ToString().ToLowerInvariant();
}