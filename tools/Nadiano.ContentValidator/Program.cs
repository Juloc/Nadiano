using System.Text.Json;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;
using Nadiano.Core.Content.Validation;

if (args.Length > 0 && args[0] == "--generate")
{
    return GenerateExpectedEvents(args.Length > 1 ? args[1] : "content");
}

var contentRoot = args.Length > 0 ? args[0] : "content";
var fullContentRoot = Path.GetFullPath(contentRoot);

if (!Directory.Exists(fullContentRoot))
{
    Console.Error.WriteLine($"Content root not found: {fullContentRoot}");
    return 1;
}

var repository = new BundledContentRepository(fullContentRoot);
var validator = new ContentValidator(repository);
var result = validator.ValidateAll();

if (result.IsValid)
{
    Console.WriteLine($"Content validation passed. Content root: {fullContentRoot}");
    return 0;
}

Console.WriteLine($"Content validation found {result.Issues.Count} issue(s):");
foreach (var issue in result.Issues)
{
    Console.WriteLine($"  {issue}");
}

return 1;

// (Re)writes expected-events.json for every lesson that declares Notation, from its current
// score.musicxml, so authored MusicXML and committed events can never silently drift apart
// (see the "regenerate it from score.musicxml" guidance in LessonManifestValidator).
static int GenerateExpectedEvents(string contentRoot)
{
    var fullContentRoot = Path.GetFullPath(contentRoot);
    if (!Directory.Exists(fullContentRoot))
    {
        Console.Error.WriteLine($"Content root not found: {fullContentRoot}");
        return 1;
    }

    var repository = new BundledContentRepository(fullContentRoot);
    var written = 0;
    var failed = 0;

    foreach (var courseId in repository.DiscoverCourseIds())
    {
        foreach (var lessonId in repository.DiscoverLessonIds(courseId))
        {
            var lesson = repository.LoadLesson(courseId, lessonId);
            if (lesson.Notation is not { } notation)
            {
                continue;
            }

            var notationPath = Path.Combine(repository.GetLessonDirectory(courseId, lessonId), notation.Path);
            if (!File.Exists(notationPath))
            {
                Console.Error.WriteLine($"[{courseId}/{lessonId}] notation file not found: {notationPath}");
                failed++;
                continue;
            }

            var musicXml = File.ReadAllText(notationPath);
            var generation = MusicXmlExpectedEventGenerator.Generate(
                musicXml,
                ToHandMapping(notation.PartMapping),
                lesson.Practice?.TargetTempo ?? 90);

            if (!generation.Success)
            {
                Console.Error.WriteLine($"[{courseId}/{lessonId}] could not generate expected events:");
                foreach (var issue in generation.UnsupportedConstructs)
                {
                    Console.Error.WriteLine($"    {issue}");
                }
                failed++;
                continue;
            }

            var json = JsonSerializer.Serialize(generation.Document, ContentJsonOptions.Default);
            File.WriteAllText(repository.GetExpectedEventsPath(courseId, lessonId), json);
            Console.WriteLine($"[{courseId}/{lessonId}] wrote expected-events.json");
            written++;
        }
    }

    Console.WriteLine($"Done. {written} written, {failed} failed.");
    return failed == 0 ? 0 : 1;
}

static IReadOnlyDictionary<string, Hand>? ToHandMapping(IReadOnlyDictionary<string, string> partMapping)
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
