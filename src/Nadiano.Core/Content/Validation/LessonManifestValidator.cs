using System.Text.Json;

using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Content.Validation;

public static class LessonManifestValidator
{
    public static ContentValidationResult Validate(
        LessonManifest lesson,
        string manifestPath,
        BundledContentRepository repository,
        string courseId,
        IReadOnlySet<string> knownSkillIds,
        IReadOnlySet<string> knownLessonIdsInCourse,
        IReadOnlyList<string> courseSupportedLanguages)
    {
        var result = new ContentValidationResult();

        ValidateSchema(lesson, manifestPath, result);
        ValidateReferences(lesson, manifestPath, knownSkillIds, knownLessonIdsInCourse, result);
        ValidateNotationAndPractice(lesson, manifestPath, repository, courseId, result);
        ValidateTechniqueMedia(lesson, manifestPath, repository, courseId, result);
        ValidateLocalization(lesson, manifestPath, repository, courseId, courseSupportedLanguages, result);
        ValidateLicense(lesson, manifestPath, repository, courseId, result);
        ValidateRuntimeReadiness(lesson, manifestPath, repository, courseId, result);

        return result;
    }

    private static void ValidateSchema(LessonManifest lesson, string manifestPath, ContentValidationResult result)
    {
        if (lesson.SchemaVersion != 1)
        {
            result.Add(manifestPath, nameof(lesson.SchemaVersion), $"Unsupported schema version {lesson.SchemaVersion}.");
        }

        if (string.IsNullOrWhiteSpace(lesson.Id))
        {
            result.Add(manifestPath, nameof(lesson.Id), "Lesson id must not be blank.");
        }

        if (lesson.Order < 0)
        {
            result.Add(manifestPath, nameof(lesson.Order), "Order must not be negative.");
        }

        if (lesson.EstimatedMinutes <= 0)
        {
            result.Add(manifestPath, nameof(lesson.EstimatedMinutes), "EstimatedMinutes must be positive.");
        }
    }

    private static void ValidateReferences(
        LessonManifest lesson,
        string manifestPath,
        IReadOnlySet<string> knownSkillIds,
        IReadOnlySet<string> knownLessonIdsInCourse,
        ContentValidationResult result)
    {
        foreach (var skillId in lesson.Skills)
        {
            if (!knownSkillIds.Contains(skillId))
            {
                result.Add(manifestPath, nameof(lesson.Skills), $"Unknown skill id '{skillId}'.");
            }
        }

        foreach (var prerequisiteId in lesson.Prerequisites)
        {
            if (prerequisiteId == lesson.Id)
            {
                result.Add(manifestPath, nameof(lesson.Prerequisites), "A lesson cannot list itself as a prerequisite.");
            }
            else if (!knownLessonIdsInCourse.Contains(prerequisiteId))
            {
                result.Add(manifestPath, nameof(lesson.Prerequisites), $"Unknown prerequisite lesson id '{prerequisiteId}'.");
            }
        }

        if (lesson.Assessment is { } assessment)
        {
            foreach (var selfCheckSkillId in assessment.SelfChecks)
            {
                if (!knownSkillIds.Contains(selfCheckSkillId))
                {
                    result.Add(manifestPath, "Assessment.SelfChecks", $"Unknown self-check skill id '{selfCheckSkillId}'.");
                }
            }
        }
    }

    private static void ValidateNotationAndPractice(
        LessonManifest lesson,
        string manifestPath,
        BundledContentRepository repository,
        string courseId,
        ContentValidationResult result)
    {
        if (lesson.Notation is { } notation)
        {
            var notationPath = Path.Combine(repository.GetLessonDirectory(courseId, lesson.Id), notation.Path);
            if (!File.Exists(notationPath))
            {
                result.Add(manifestPath, "Notation.Path", $"Referenced notation file does not exist: {notation.Path}");
            }
        }

        if (lesson.Practice is { } practice)
        {
            if (!practice.SupportedModes.Contains(practice.DefaultMode))
            {
                result.Add(manifestPath, "Practice.DefaultMode", "DefaultMode must be one of SupportedModes.");
            }

            foreach (var section in practice.Sections)
            {
                if (section.FromMeasure > section.ToMeasure)
                {
                    result.Add(manifestPath, "Practice.Sections", $"Section '{section.Id}' has FromMeasure greater than ToMeasure.");
                }
            }
        }
    }

    private static void ValidateTechniqueMedia(
        LessonManifest lesson,
        string manifestPath,
        BundledContentRepository repository,
        string courseId,
        ContentValidationResult result)
    {
        if (lesson.Technique is not { } technique)
        {
            return;
        }

        if (technique.Views.Count == 0)
        {
            result.Add(manifestPath, "Technique.Views", "Technique media must declare at least one view.");
        }

        var lessonDirectory = repository.GetLessonDirectory(courseId, lesson.Id);

        foreach (var view in technique.Views)
        {
            var mediaPath = Path.Combine(lessonDirectory, view.Path);
            if (!File.Exists(mediaPath))
            {
                result.Add(manifestPath, "Technique.Views", $"Referenced technique media file does not exist: {view.Path}");
            }

            if (view.Poster is { } poster && !File.Exists(Path.Combine(lessonDirectory, poster)))
            {
                result.Add(manifestPath, "Technique.Views", $"Referenced technique media poster does not exist: {poster}");
            }
        }
    }

    private static void ValidateLocalization(
        LessonManifest lesson,
        string manifestPath,
        BundledContentRepository repository,
        string courseId,
        IReadOnlyList<string> courseSupportedLanguages,
        ContentValidationResult result)
    {
        foreach (var culture in courseSupportedLanguages)
        {
            var textPath = repository.GetLocalizedTextPath(courseId, lesson.Id, culture, lesson.Localization.Directory);
            if (!File.Exists(textPath))
            {
                result.Add(manifestPath, "Localization", $"Missing localization file for culture '{culture}': {textPath}");
                continue;
            }

            try
            {
                var text = repository.LoadLocalizedText(courseId, lesson.Id, culture, lesson.Localization.Directory);
                if (string.IsNullOrWhiteSpace(text.Title))
                {
                    result.Add(textPath, nameof(text.Title), "Title must not be blank.");
                }
            }
            catch (InvalidDataException ex)
            {
                result.Add(textPath, null, ex.Message);
            }
        }
    }

    private static void ValidateLicense(
        LessonManifest lesson,
        string manifestPath,
        BundledContentRepository repository,
        string courseId,
        ContentValidationResult result)
    {
        var attribution = repository.LoadAttribution(courseId, lesson.Id);
        if (attribution is null)
        {
            result.Add(manifestPath, nameof(lesson.Attribution), "Missing attribution.json.");
            return;
        }

        if (string.IsNullOrWhiteSpace(attribution.License))
        {
            result.Add(repository.GetAttributionPath(courseId, lesson.Id), nameof(attribution.License), "License must not be blank.");
        }

        if (string.IsNullOrWhiteSpace(attribution.Source))
        {
            result.Add(repository.GetAttributionPath(courseId, lesson.Id), nameof(attribution.Source), "Source must not be blank.");
        }
    }

    private static void ValidateRuntimeReadiness(
        LessonManifest lesson,
        string manifestPath,
        BundledContentRepository repository,
        string courseId,
        ContentValidationResult result)
    {
        if (lesson.Notation is null || lesson.Practice is null)
        {
            return;
        }

        var expectedEventsPath = repository.GetExpectedEventsPath(courseId, lesson.Id);
        if (!File.Exists(expectedEventsPath))
        {
            result.Add(
                manifestPath,
                "ExpectedEvents",
                "Lesson declares notation and practice modes but has no expected-events.json — it cannot generate a scored practice session.");
            return;
        }

        var notationPath = Path.Combine(repository.GetLessonDirectory(courseId, lesson.Id), lesson.Notation.Path);
        if (!File.Exists(notationPath))
        {
            return; // already reported by ValidateNotationAndPractice
        }

        var musicXml = File.ReadAllText(notationPath);
        var generation = MusicXmlExpectedEventGenerator.Generate(musicXml, ToHandMapping(lesson.Notation.PartMapping));

        if (!generation.Success)
        {
            foreach (var issue in generation.UnsupportedConstructs)
            {
                result.Add(notationPath, "ExpectedEvents", $"Cannot generate expected events: {issue}");
            }
            return;
        }

        var committed = repository.LoadExpectedEvents(courseId, lesson.Id);
        var generatedJson = JsonSerializer.Serialize(generation.Document, ContentJsonOptions.Default);
        var committedJson = JsonSerializer.Serialize(committed, ContentJsonOptions.Default);

        if (generatedJson != committedJson)
        {
            result.Add(
                expectedEventsPath,
                null,
                "expected-events.json does not match what the MusicXML source currently generates — regenerate it from score.musicxml.");
        }
    }

    private static IReadOnlyDictionary<string, Hand>? ToHandMapping(IReadOnlyDictionary<string, string> partMapping)
    {
        if (partMapping.Count == 0)
        {
            return null;
        }

        var result = new Dictionary<string, Hand>();
        foreach (var (partId, role) in partMapping)
        {
            if (role.Contains("left", StringComparison.OrdinalIgnoreCase))
            {
                result[partId] = Hand.Left;
            }
            else if (role.Contains("right", StringComparison.OrdinalIgnoreCase))
            {
                result[partId] = Hand.Right;
            }
            else if (role.Contains("both", StringComparison.OrdinalIgnoreCase))
            {
                result[partId] = Hand.Both;
            }
        }

        return result;
    }
}