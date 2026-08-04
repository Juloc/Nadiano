using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Content.Validation;

/// <summary>Runs every validation level across all bundled content (docs/CONTENT_MODEL.md §14).</summary>
public sealed class ContentValidator(BundledContentRepository repository)
{
    public ContentValidationResult ValidateAll()
    {
        var result = new ContentValidationResult();

        var knownSkillIds = LoadSkillCatalogueInto(result);

        foreach (var courseId in repository.DiscoverCourseIds())
        {
            ValidateCourse(courseId, knownSkillIds, result);
        }

        return result;
    }

    private HashSet<string> LoadSkillCatalogueInto(ContentValidationResult result)
    {
        try
        {
            var catalogue = repository.LoadSkillCatalogue();
            result.Merge(SkillCatalogueValidator.Validate(catalogue, repository.SkillsCataloguePath));
            return catalogue.Skills.Select(s => s.Id).ToHashSet();
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            result.Add(repository.SkillsCataloguePath, null, ex.Message);
            return [];
        }
    }

    private void ValidateCourse(string courseId, IReadOnlySet<string> knownSkillIds, ContentValidationResult result)
    {
        var manifestPath = repository.GetCourseManifestPath(courseId);

        CourseManifest course;
        try
        {
            course = repository.LoadCourse(courseId);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            result.Add(manifestPath, null, ex.Message);
            return;
        }

        var lessonIds = repository.DiscoverLessonIds(courseId);
        var knownLessonIds = lessonIds.ToHashSet();

        result.Merge(CourseManifestValidator.Validate(course, manifestPath, knownLessonIds));

        foreach (var lessonId in lessonIds)
        {
            ValidateLesson(courseId, lessonId, knownSkillIds, knownLessonIds, course.SupportedLanguages, result);
        }
    }

    private void ValidateLesson(
        string courseId,
        string lessonId,
        IReadOnlySet<string> knownSkillIds,
        IReadOnlySet<string> knownLessonIds,
        IReadOnlyList<string> supportedLanguages,
        ContentValidationResult result)
    {
        var manifestPath = repository.GetLessonManifestPath(courseId, lessonId);

        LessonManifest lesson;
        try
        {
            lesson = repository.LoadLesson(courseId, lessonId);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            result.Add(manifestPath, null, ex.Message);
            return;
        }

        result.Merge(LessonManifestValidator.Validate(lesson, manifestPath, repository, courseId, knownSkillIds, knownLessonIds, supportedLanguages));
    }
}