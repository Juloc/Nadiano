using Nadiano.Core.Beta;

namespace Nadiano.Core.Tests.Beta;

public class BetaCurriculumTests
{
    [Fact]
    public void Catalogue_MeetsBetaQuantityAndLanguageGate()
    {
        var catalogue = BetaCurriculumCatalogue.Create();

        Assert.Equal(45, catalogue.Lessons.Count);
        Assert.Equal(100, catalogue.Exercises.Count);
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
