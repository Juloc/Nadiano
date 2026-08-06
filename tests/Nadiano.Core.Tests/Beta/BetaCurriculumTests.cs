using Nadiano.Core.Beta;

namespace Nadiano.Core.Tests.Beta;

public class BetaCurriculumTests
{
    [Fact]
    public void Catalogue_MeetsReleaseQuantityAndLanguageGate()
    {
        var catalogue = BetaCurriculumCatalogue.Create();

        Assert.True(catalogue.Lessons.Count >= 80);
        Assert.True(catalogue.Exercises.Count >= 180);
        Assert.True(catalogue.Lessons.Count(item => item.ActivityKind == "ear") >= 12);
        Assert.True(catalogue.Lessons.Count(item => item.ActivityKind == "repertoire") >= 8);
        Assert.Contains(catalogue.Lessons, item => item.Stage == "E1");
        Assert.Empty(BetaCurriculumCatalogue.Validate(catalogue));
    }

    [Fact]
    public void Catalogue_IsDeterministic()
    {
        var first = BetaCurriculumCatalogue.Create();
        var second = BetaCurriculumCatalogue.Create();

        Assert.Equal(first.Lessons, second.Lessons);
        Assert.Equal(first.Exercises, second.Exercises);
    }
}
