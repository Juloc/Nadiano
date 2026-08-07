using System.Globalization;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Core.Beta;
using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;
using Nadiano.Web.Features.Library;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Practice;

public class IndexModel(
    ContentCatalogue catalogue,
    CourseProgressService progress,
    BundledContentRepository content,
    PrivateLibraryService library,
    CurrentProfileAccessor profiles) : PageModel
{
    public bool HasLesson { get; private set; }
    public bool IsPrivateLibraryItem { get; private set; }
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

    public async Task<IActionResult> OnGetAsync(
        string? lessonId,
        Guid? libraryItemId,
        string? repertoireId,
        CancellationToken cancellationToken)
    {
        if (libraryItemId.HasValue)
        {
            return await LoadPrivateLibraryItemAsync(libraryItemId.Value, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(repertoireId))
        {
            return LoadRepertoireItem(repertoireId);
        }

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
            .Where(IsSupportedMode)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        AssessmentCategoriesJson = JsonSerializer.Serialize(
            lesson.Assessment.Categories.Select(CategoryCode).ToArray());

        return Page();
    }

    public string ModeDisplayName(string mode) => mode switch
    {
        "wait" => T("Warten", "Tunggu"),
        "loop" => T("Abschnitt wiederholen", "Ulangi bagian"),
        "hands-separate" => T("Hände getrennt", "Tangan terpisah"),
        "rhythm" => T("Nur Rhythmus", "Ritme saja"),
        "tempo-ladder" => T("Tempo-Leiter", "Tangga tempo"),
        "listen-and-copy" => T("Hören und nachspielen", "Dengar dan tirukan"),
        "performance" => T("Durchspielen", "Pertunjukan"),
        "sight-reading" => T("Vom Blatt", "Baca langsung"),
        _ => mode,
    };

    public string T(string german, string indonesian) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? indonesian : german;

    private IActionResult LoadRepertoireItem(string repertoireId)
    {
        var piece = ReleaseContentCatalogue.FindRepertoire(repertoireId);
        if (piece is null)
        {
            return NotFound();
        }

        var indonesian = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id";
        HasLesson = true;
        LessonId = $"repertoire:{piece.Id}";
        LessonTitle = indonesian ? piece.TitleId : piece.TitleDe;
        ContentVersion = "release-1.0";
        ScoreUrl = $"/api/beta/repertoire/{Uri.EscapeDataString(piece.Id)}/score";
        ExpectedEventsUrl = $"/api/beta/repertoire/{Uri.EscapeDataString(piece.Id)}/expected-events";
        TargetTempo = piece.TargetTempoBpm;
        CountInMeasures = 1;
        DefaultMode = "sight-reading";
        SupportedModes = ["wait", "loop", "rhythm", "tempo-ladder", "listen-and-copy", "performance", "sight-reading"];
        AssessmentCategoriesJson = JsonSerializer.Serialize(new[] { "pitch", "onset", "duration", "steadiness", "dynamics" });
        return Page();
    }

    private async Task<IActionResult> LoadPrivateLibraryItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        var resolved = await library.ResolveFileAsync(profileId, itemId, original: false, cancellationToken);
        if (resolved is null)
        {
            return NotFound();
        }

        var metadata = JsonSerializer.Deserialize<LibraryItemMetadata>(resolved.Value.Item.MetadataJson)
            ?? new LibraryItemMetadata(1, 1, 0, 1, 1, 90);

        HasLesson = true;
        IsPrivateLibraryItem = true;
        LessonId = $"private:{itemId:N}";
        LessonTitle = resolved.Value.Item.DisplayTitle;
        ContentVersion = $"private-{resolved.Value.Item.Version}";
        ScoreUrl = $"/api/library/{itemId}/score";
        ExpectedEventsUrl = $"/api/library/{itemId}/expected-events";
        TargetTempo = metadata.TargetTempoBpm;
        CountInMeasures = 1;
        DefaultMode = "wait";
        SupportedModes = ["wait", "loop", "hands-separate", "rhythm", "tempo-ladder", "listen-and-copy", "performance", "sight-reading"];
        AssessmentCategoriesJson = JsonSerializer.Serialize(new[] { "pitch", "onset", "duration", "steadiness", "dynamics" });
        return Page();
    }

    private static bool IsSupportedMode(string mode) =>
        mode is "wait" or "loop" or "hands-separate" or "rhythm" or "tempo-ladder" or "listen-and-copy" or "performance" or "sight-reading";

    private static string ContentUrl(string lessonId, string relativePath) =>
        $"/api/content/lessons/{Uri.EscapeDataString(lessonId)}/files/{relativePath}";

    private static string ModeCode(PracticeMode mode) => mode switch
    {
        PracticeMode.Wait => "wait",
        PracticeMode.Loop => "loop",
        PracticeMode.HandsSeparate => "hands-separate",
        PracticeMode.Rhythm => "rhythm",
        PracticeMode.TempoLadder => "tempo-ladder",
        PracticeMode.ListenAndCopy => "listen-and-copy",
        PracticeMode.Performance => "performance",
        PracticeMode.SightReading => "sight-reading",
        _ => "wait",
    };

    private static string CategoryCode(AssessmentCategory category) => category.ToString().ToLowerInvariant();
}
