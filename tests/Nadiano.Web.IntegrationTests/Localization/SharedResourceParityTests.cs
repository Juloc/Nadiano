using System.Collections;
using System.Globalization;
using System.Resources;

using Nadiano.Web.Infrastructure.Localization;

namespace Nadiano.Web.IntegrationTests.Localization;

public class SharedResourceParityTests
{
    [Fact]
    public void German_And_Indonesian_Resources_Have_The_Same_Keys()
    {
        var germanKeys = GetKeys("de");
        var indonesianKeys = GetKeys("id");

        Assert.NotEmpty(germanKeys);

        var missingInIndonesian = germanKeys.Except(indonesianKeys).ToArray();
        var missingInGerman = indonesianKeys.Except(germanKeys).ToArray();

        Assert.True(
            missingInIndonesian.Length == 0 && missingInGerman.Length == 0,
            $"Localization key parity broken.\n" +
            $"Missing in id: {string.Join(", ", missingInIndonesian)}\n" +
            $"Missing in de: {string.Join(", ", missingInGerman)}");
    }

    [Theory]
    [InlineData("de")]
    [InlineData("id")]
    public void No_Resource_Value_Is_Blank(string culture)
    {
        var resourceManager = new ResourceManager(
            typeof(SharedResource).FullName!,
            typeof(SharedResource).Assembly);

        var resourceSet = resourceManager.GetResourceSet(new CultureInfo(culture), createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"No resource set found for culture '{culture}'.");

        var blankKeys = resourceSet
            .Cast<DictionaryEntry>()
            .Where(entry => string.IsNullOrWhiteSpace(entry.Value as string))
            .Select(entry => (string)entry.Key)
            .ToArray();

        Assert.True(blankKeys.Length == 0, $"Blank resource values for culture '{culture}': {string.Join(", ", blankKeys)}");
    }

    private static HashSet<string> GetKeys(string culture)
    {
        var resourceManager = new ResourceManager(
            typeof(SharedResource).FullName!,
            typeof(SharedResource).Assembly);

        var resourceSet = resourceManager.GetResourceSet(new CultureInfo(culture), createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"No resource set found for culture '{culture}'.");

        return resourceSet
            .Cast<DictionaryEntry>()
            .Select(entry => (string)entry.Key)
            .ToHashSet();
    }
}