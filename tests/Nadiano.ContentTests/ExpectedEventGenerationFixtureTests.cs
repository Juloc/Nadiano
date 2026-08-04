using System.Text.Json;

using Nadiano.Core.Content;

namespace Nadiano.ContentTests;

/// <summary>
/// Compares generated output against a committed expected-events.json fixture
/// (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-013 step 6), so an unintended change
/// in generation logic shows up as a diff review must explain, not a silent behavior change.
/// </summary>
public class ExpectedEventGenerationFixtureTests
{
    private static string FixturePath(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "expected-events", name);

    [Fact]
    public void Generate_MatchesTheCommittedFixture_ForTheDemoScale()
    {
        var musicXml = File.ReadAllText(FixturePath("demo-scale.musicxml"));
        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));

        var actualJson = JsonSerializer.Serialize(result.Document, new JsonSerializerOptions(ContentJsonOptions.Default) { WriteIndented = true });
        var expectedJson = File.ReadAllText(FixturePath("demo-scale.expected-events.json"));

        var actualNormalized = JsonSerializer.Serialize(JsonDocument.Parse(actualJson).RootElement);
        var expectedNormalized = JsonSerializer.Serialize(JsonDocument.Parse(expectedJson).RootElement);

        Assert.Equal(expectedNormalized, actualNormalized);
    }
}