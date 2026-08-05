using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;

namespace Nadiano.Web.Infrastructure.Courses;

/// <summary>
/// Loads every bundled course and lesson into memory once at startup. Bundled
/// content is small, static and read-only at runtime (docs/CONTENT_MODEL.md
/// §15), so a singleton in-memory index avoids re-reading disk per request.
/// </summary>
public sealed class ContentCatalogue
{
    private readonly Dictionary<string, CourseManifest> _courses = new();
    private readonly Dictionary<string, Dictionary<string, LessonManifest>> _lessonsByCourse = new();
    private readonly Dictionary<string, string> _courseIdByLessonId = new();

    public ContentCatalogue(BundledContentRepository repository)
    {
        foreach (var courseId in repository.DiscoverCourseIds())
        {
            _courses[courseId] = repository.LoadCourse(courseId);

            var lessons = new Dictionary<string, LessonManifest>();
            foreach (var lessonId in repository.DiscoverLessonIds(courseId))
            {
                lessons[lessonId] = repository.LoadLesson(courseId, lessonId);
                _courseIdByLessonId[lessonId] = courseId;
            }

            _lessonsByCourse[courseId] = lessons;
        }
    }

    public bool TryGetCourse(
        string courseId, out CourseManifest course, out IReadOnlyDictionary<string, LessonManifest> lessonsById)
    {
        if (!_courses.TryGetValue(courseId, out var found))
        {
            course = null!;
            lessonsById = null!;
            return false;
        }

        course = found;
        lessonsById = _lessonsByCourse[courseId];
        return true;
    }

    public IReadOnlyList<string> CourseIds => _courses.Keys.ToArray();

    public (string CourseId, LessonManifest Lesson)? FindLesson(string lessonId) =>
        _courseIdByLessonId.TryGetValue(lessonId, out var courseId)
            ? (courseId, _lessonsByCourse[courseId][lessonId])
            : null;
}