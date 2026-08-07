using Nadiano.Core.Beta;

namespace Nadiano.Core.Tests.Beta;

public class BetaCurriculumTests
{
    [Fact]
    public void Catalogue_MeetsRoadmapOnePointZeroGate()
    {
        var catalogue = BetaCurriculumCatalogue.Create();
        var releaseContent = ReleaseContentCatalogue.Create();

        Assert.True(catalogue.Lessons.Count >= 60);
        Assert.True(catalogue.Exercises.Count(item => item.Kind == GeneratedExerciseKind.Rhythm) >= 120);
        Assert.True(catalogue.Exercises.Count(item => item.Kind == GeneratedExerciseKind.Reading) >= 80);
        Assert.Contains(catalogue.Lessons, item => item.Id == "course-e1-stage-check");
        Assert.True(releaseContent.EarTasks.Count >= 60);
        Assert.True(releaseContent.Repertoire.Count(item => item.SourceKind == ReleaseContentCatalogue.OriginalSourceKind) >= 24);
        Assert.True(releaseContent.Repertoire.Count(item => item.SourceKind == ReleaseContentCatalogue.PublicDomainSourceKind) >= 12);
        Assert.Empty(BetaCurriculumCatalogue.Validate(catalogue));
        Assert.Empty(ReleaseContentCatalogue.Validate(releaseContent));
    }

    [Fact]
    public void Catalogue_IsDeterministic()
    {
        var first = BetaCurriculumCatalogue.Create();
        var second = BetaCurriculumCatalogue.Create();
        var firstRelease = ReleaseContentCatalogue.Create();
        var secondRelease = ReleaseContentCatalogue.Create();

        Assert.Equal(first.Lessons, second.Lessons);
        Assert.Equal(first.Exercises, second.Exercises);
        Assert.Equal(firstRelease.EarTasks, secondRelease.EarTasks);
        Assert.Equal(firstRelease.Repertoire, secondRelease.Repertoire);
    }

    [Fact]
    public void Repertoire_GeneratesValidMusicXmlExpectedEvents()
    {
        foreach (var piece in ReleaseContentCatalogue.Create().Repertoire)
        {
            var musicXml = ReleaseContentCatalogue.CreateMusicXml(piece);
            var generated = Nadiano.Core.Content.MusicXmlExpectedEventGenerator.Generate(
                musicXml,
                defaultTempoBpm: piece.TargetTempoBpm);

            Assert.NotNull(generated.Document);
            Assert.Equal(piece.MidiNotes.Count, generated.Document!.Events.Count);
        }
    }
}
