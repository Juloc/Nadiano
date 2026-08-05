using Nadiano.Core.Content;
using Nadiano.Web.Infrastructure.Courses;

namespace Nadiano.Web.Features.Content;

/// <summary>
/// Serves only files declared by a bundled lesson manifest. Technique media,
/// the canonical notation file and its generated expected-events document use
/// the same traversal-safe endpoint; arbitrary relative paths are rejected.
/// </summary>
public static class ContentMediaEndpoints
{
    private static readonly Dictionary<string, string> ContentTypesByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".json"] = "application/json",
        [".musicxml"] = "application/vnd.recordare.musicxml+xml",
        [".webm"] = "video/webm",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        [".wav"] = "audio/wav",
    };

    public static void MapContentMediaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/content/lessons/{lessonId}/files/{*relativePath}", GetMediaAsync);
    }

    private static Task<IResult> GetMediaAsync(
        string lessonId,
        string relativePath,
        ContentCatalogue catalogue,
        BundledContentRepository repository)
    {
        var found = catalogue.FindLesson(lessonId);
        if (found is null)
        {
            return Task.FromResult(Results.NotFound());
        }

        var (courseId, lesson) = found.Value;
        var declaredTechniquePath = lesson.Technique?.Views
            .Any(view => view.Path == relativePath || view.Poster == relativePath) == true;
        var declaredNotationPath = lesson.Notation?.Path == relativePath;
        var declaredExpectedEventsPath = lesson.Notation is not null && relativePath == "expected-events.json";

        if (!declaredTechniquePath && !declaredNotationPath && !declaredExpectedEventsPath)
        {
            return Task.FromResult(Results.NotFound());
        }

        var absolutePath = Path.Combine(repository.GetLessonDirectory(courseId, lessonId), relativePath);
        if (!File.Exists(absolutePath))
        {
            return Task.FromResult(Results.NotFound());
        }

        var contentType = ContentTypesByExtension.GetValueOrDefault(
            Path.GetExtension(relativePath),
            "application/octet-stream");
        return Task.FromResult(Results.File(absolutePath, contentType));
    }
}