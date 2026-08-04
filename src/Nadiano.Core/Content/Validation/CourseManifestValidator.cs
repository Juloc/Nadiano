using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Content.Validation;

public static class CourseManifestValidator
{
    public static ContentValidationResult Validate(CourseManifest course, string manifestPath, IReadOnlySet<string> knownLessonIds)
    {
        var result = new ContentValidationResult();

        if (course.SupportedLanguages.Count == 0)
        {
            result.Add(manifestPath, nameof(course.SupportedLanguages), "At least one supported language is required.");
        }
        else if (!course.SupportedLanguages.Contains(course.DefaultLanguage))
        {
            result.Add(manifestPath, nameof(course.DefaultLanguage), "DefaultLanguage must be included in SupportedLanguages.");
        }

        var seenStageIds = new HashSet<string>();
        var seenItems = new HashSet<string>();

        foreach (var stage in course.Stages)
        {
            if (!seenStageIds.Add(stage.Id))
            {
                result.Add(manifestPath, nameof(stage.Id), $"Duplicate stage id '{stage.Id}'.");
            }

            foreach (var item in stage.Items)
            {
                if (!knownLessonIds.Contains(item))
                {
                    result.Add(manifestPath, nameof(stage.Items), $"Stage '{stage.Id}' references unknown lesson id '{item}'.");
                }

                if (!seenItems.Add(item))
                {
                    result.Add(manifestPath, nameof(stage.Items), $"Lesson id '{item}' appears in more than one stage.");
                }
            }
        }

        return result;
    }
}