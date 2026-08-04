using System.Text.Json;

using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Content;

/// <summary>
/// Reads bundled content from disk using the fixed layout described in
/// docs/TECHNICAL_ARCHITECTURE.md §4:
/// content/courses/{courseId}/course.json
/// content/courses/{courseId}/lessons/{lessonId}/lesson.json (+ i18n/, attribution.json, score.musicxml, expected-events.json)
/// content/skills/skills.json
/// </summary>
public sealed class BundledContentRepository(string contentRoot)
{
    public string ContentRoot { get; } = contentRoot;

    public string CoursesDirectory => Path.Combine(ContentRoot, "courses");

    public string SkillsCataloguePath => Path.Combine(ContentRoot, "skills", "skills.json");

    public IReadOnlyList<string> DiscoverCourseIds()
    {
        if (!Directory.Exists(CoursesDirectory))
        {
            return [];
        }

        return Directory.EnumerateDirectories(CoursesDirectory)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    public string GetCourseDirectory(string courseId) => Path.Combine(CoursesDirectory, courseId);

    public string GetCourseManifestPath(string courseId) => Path.Combine(GetCourseDirectory(courseId), "course.json");

    public CourseManifest LoadCourse(string courseId) =>
        Deserialize<CourseManifest>(GetCourseManifestPath(courseId));

    public IReadOnlyList<string> DiscoverLessonIds(string courseId)
    {
        var lessonsDirectory = Path.Combine(GetCourseDirectory(courseId), "lessons");
        if (!Directory.Exists(lessonsDirectory))
        {
            return [];
        }

        return Directory.EnumerateDirectories(lessonsDirectory)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    public string GetLessonDirectory(string courseId, string lessonId) =>
        Path.Combine(GetCourseDirectory(courseId), "lessons", lessonId);

    public string GetLessonManifestPath(string courseId, string lessonId) =>
        Path.Combine(GetLessonDirectory(courseId, lessonId), "lesson.json");

    public LessonManifest LoadLesson(string courseId, string lessonId) =>
        Deserialize<LessonManifest>(GetLessonManifestPath(courseId, lessonId));

    public string GetLocalizedTextPath(string courseId, string lessonId, string culture, string localizationDirectory = "i18n") =>
        Path.Combine(GetLessonDirectory(courseId, lessonId), localizationDirectory, $"{culture}.json");

    public LocalizedLessonText LoadLocalizedText(string courseId, string lessonId, string culture, string localizationDirectory = "i18n") =>
        Deserialize<LocalizedLessonText>(GetLocalizedTextPath(courseId, lessonId, culture, localizationDirectory));

    public string GetAttributionPath(string courseId, string lessonId, string fileName = "attribution.json") =>
        Path.Combine(GetLessonDirectory(courseId, lessonId), fileName);

    public AttributionInfo? LoadAttribution(string courseId, string lessonId, string fileName = "attribution.json")
    {
        var path = GetAttributionPath(courseId, lessonId, fileName);
        return File.Exists(path) ? Deserialize<AttributionInfo>(path) : null;
    }

    public string GetExpectedEventsPath(string courseId, string lessonId) =>
        Path.Combine(GetLessonDirectory(courseId, lessonId), "expected-events.json");

    public ExpectedEventDocument? LoadExpectedEvents(string courseId, string lessonId)
    {
        var path = GetExpectedEventsPath(courseId, lessonId);
        return File.Exists(path) ? Deserialize<ExpectedEventDocument>(path) : null;
    }

    public SkillCatalogue LoadSkillCatalogue() => Deserialize<SkillCatalogue>(SkillsCataloguePath);

    private static T Deserialize<T>(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Required content file was not found: {path}", path);
        }

        var json = File.ReadAllText(path);
        try
        {
            return JsonSerializer.Deserialize<T>(json, ContentJsonOptions.Default)
                ?? throw new InvalidDataException($"Content file deserialized to null: {path}");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Content file is not valid JSON for {typeof(T).Name}: {path} ({ex.Message})", ex);
        }
    }
}