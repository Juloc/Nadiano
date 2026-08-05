using Nadiano.Core.Content;
using Nadiano.Web.Infrastructure.Courses;

namespace Nadiano.Web.Features.Content;

/// <summary>
/// Serves technique demonstration media referenced from a lesson manifest
/// (docs/CONTENT_MODEL.md §9). Only paths the manifest itself declares
/// (a view's path or poster) are ever served — the request never resolves
/// an arbitrary filename, so path traversal is not reachable
/// (docs/CONTENT_MODEL.md §16).
/// </summary>
public static class ContentMediaEndpoints
{
    private static readonly Dictionary<string, string> ContentTypesByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
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
        string lessonId, string relativePath, ContentCatalogue catalogue, BundledContentRepository repository)
    {
        var found = catalogue.FindLesson(lessonId);
        if (found is null || found.Value.Lesson.Technique is not { } technique)
        {
            return Task.FromResult(Results.NotFound());
        }

        var isDeclared = technique.Views.Any(view => view.Path == relativePath || view.Poster == relativePath);
        if (!isDeclared)
        {
            return Task.FromResult(Results.NotFound());
        }

        var (courseId, _) = found.Value;
        var absolutePath = Path.Combine(repository.GetLessonDirectory(courseId, lessonId), relativePath);
        if (!File.Exists(absolutePath))
        {
            return Task.FromResult(Results.NotFound());
        }

        var contentType = ContentTypesByExtension.GetValueOrDefault(Path.GetExtension(relativePath), "application/octet-stream");
        return Task.FromResult(Results.File(absolutePath, contentType));
    }
}
