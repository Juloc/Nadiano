using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Learn;

public class LessonModel(
    ContentCatalogue catalogue,
    CourseProgressService progress,
    BundledContentRepository content,
    CurrentProfileAccessor profiles) : PageModel
{
    public string LessonId { get; private set; } = string.Empty;

    public string ContentVersion { get; private set; } = string.Empty;

    public LocalizedLessonText Text { get; private set; } = null!;

    public bool HasNotationPractice { get; private set; }

    public IReadOnlyList<string> SelfCheckSkillIds { get; private set; } = [];

    public IReadOnlyList<MediaViewModel> MediaViews { get; private set; } = [];

    public bool MediaLoops { get; private set; }

    public sealed record MediaViewModel(string Kind, string Url, string? PosterUrl, string MediaTag);

    public async Task<IActionResult> OnGetAsync(string lessonId)
    {
        var found = catalogue.FindLesson(lessonId);
        if (found is null)
        {
            return NotFound();
        }

        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext);
        if (!await progress.IsLessonAvailableAsync(profileId, lessonId))
        {
            return RedirectToPage("/Learn/Index");
        }

        var (courseId, lesson) = found.Value;
        var culture = CultureInfo.CurrentUICulture.Name;

        LessonId = lessonId;
        ContentVersion = catalogue.TryGetCourse(courseId, out var course, out _) ? course.Version : string.Empty;
        Text = content.LoadLocalizedText(courseId, lessonId, culture);
        HasNotationPractice = lesson.Notation is not null;
        SelfCheckSkillIds = lesson.Assessment?.SelfChecks ?? [];

        if (lesson.Technique is { } technique)
        {
            MediaLoops = technique.Loop;
            MediaViews = technique.Views
                .Select(view => new MediaViewModel(
                    view.Kind,
                    MediaUrl(lessonId, view.Path),
                    view.Poster is null ? null : MediaUrl(lessonId, view.Poster),
                    MediaTagFor(view.Path)))
                .ToArray();
        }

        return Page();
    }

    private static string MediaUrl(string lessonId, string relativePath) =>
        $"/api/content/lessons/{Uri.EscapeDataString(lessonId)}/files/{relativePath}";

    // Alpha technique media is illustration (svg/webp) or reference audio (wav), not filmed
    // video — see the note on TechniqueMediaMetadata. "video" stays supported for later content.
    private static string MediaTagFor(string relativePath) => Path.GetExtension(relativePath).ToLowerInvariant() switch
    {
        ".svg" or ".webp" or ".png" or ".jpg" or ".jpeg" => "image",
        ".wav" or ".ogg" or ".mp3" => "audio",
        _ => "video",
    };
}
