using System.Text.Json;

using Nadiano.Core.Content;

namespace Nadiano.Core.Tests.Content;

public class MusicXmlExpectedEventGeneratorTests
{
    private const string AttributesBlock = """
        <attributes>
          <divisions>1</divisions>
          <key><fifths>0</fifths></key>
          <time><beats>4</beats><beat-type>4</beat-type></time>
          <clef><sign>G</sign><line>2</line></clef>
        </attributes>
        """;

    private static string Wrap(string measuresXml) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <score-partwise version="4.0">
          <part-list><score-part id="P1"><part-name>Piano</part-name></score-part></part-list>
          <part id="P1">
            {measuresXml}
          </part>
        </score-partwise>
        """;

    [Fact]
    public void Generate_ProducesOneEventPerNote_ForACleanScale()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><pitch><step>D</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><pitch><step>E</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><pitch><step>F</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));
        var events = result.Document!.Events;
        Assert.Equal(4, events.Count);
        Assert.Equal([60, 62, 64, 65], events.Select(e => e.Pitches.Single()).ToArray());
        Assert.Equal([0, 1, 2, 3], events.Select(e => e.Beat).ToArray());
        Assert.Equal(["m1-v1-n1", "m1-v1-n2", "m1-v1-n3", "m1-v1-n4"], events.Select(e => e.Id).ToArray());
    }

    [Fact]
    public void Generate_SkipsRests_ButLeavesACorrectlySpacedGap()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><rest/><duration>1</duration><type>quarter</type></note>
              <note><pitch><step>E</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));
        var events = result.Document!.Events;
        Assert.Equal(2, events.Count);
        Assert.Equal(0, events[0].Beat);
        Assert.Equal(2, events[1].Beat);
    }

    [Fact]
    public void Generate_MergesChordNotesIntoOneEventWithMultiplePitches()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><chord/><pitch><step>E</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><chord/><pitch><step>G</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><pitch><step>C</step><octave>5</octave></pitch><duration>1</duration><type>quarter</type></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));
        var events = result.Document!.Events;
        Assert.Equal(2, events.Count);
        Assert.Equal([60, 64, 67], events[0].Pitches);
        Assert.Equal(1, events[1].Beat);
        Assert.Equal([72], events[1].Pitches);
    }

    [Fact]
    public void Generate_MergesATwoNoteTieIntoOneEvent_NotARepeatedAttack()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>2</duration><type>half</type><tie type="start"/></note>
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>2</duration><type>half</type><tie type="stop"/></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));
        var events = result.Document!.Events;
        Assert.Single(events);
        Assert.Equal(0, events[0].Beat);
        Assert.Equal(4, events[0].DurationBeats);
    }

    [Fact]
    public void Generate_MergesAThreeNoteTieChainIntoOneEvent()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type><tie type="start"/></note>
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type><tie type="stop"/><tie type="start"/></note>
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type><tie type="stop"/></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));
        var events = result.Document!.Events;
        Assert.Single(events);
        Assert.Equal(3, events[0].DurationBeats);
    }

    [Fact]
    public void Generate_ReadsFingeringFromNotations()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note>
                <pitch><step>C</step><octave>4</octave></pitch>
                <duration>1</duration><type>quarter</type>
                <notations><technical><fingering>2</fingering></technical></notations>
              </note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));
        Assert.Equal([2], result.Document!.Events[0].Fingering);
    }

    [Fact]
    public void Generate_ReportsUnsupportedTimeSignature_InsteadOfGuessingBeats()
    {
        var musicXml = Wrap("""
            <measure number="1">
              <attributes>
                <divisions>1</divisions>
                <time><beats>6</beats><beat-type>8</beat-type></time>
              </attributes>
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>eighth</type></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.False(result.Success);
        Assert.Contains(result.UnsupportedConstructs, i => i.Contains("beat-type"));
    }

    [Fact]
    public void Generate_ReportsUnsupportedMultiStaffParts()
    {
        var musicXml = Wrap("""
            <measure number="1">
              <attributes>
                <divisions>1</divisions>
                <time><beats>4</beats><beat-type>4</beat-type></time>
                <staves>2</staves>
              </attributes>
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.False(result.Success);
        Assert.Contains(result.UnsupportedConstructs, i => i.Contains("Multi-staff"));
    }

    [Fact]
    public void Generate_ReportsUnsupportedTuplets()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note>
                <pitch><step>C</step><octave>4</octave></pitch>
                <duration>1</duration><type>eighth</type>
                <time-modification><actual-notes>3</actual-notes><normal-notes>2</normal-notes></time-modification>
              </note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.False(result.Success);
        Assert.Contains(result.UnsupportedConstructs, i => i.Contains("Tuplet"));
    }

    [Fact]
    public void Generate_ReportsUnsupportedGraceNotes()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note>
                <grace/>
                <pitch><step>C</step><octave>4</octave></pitch>
                <type>eighth</type>
              </note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.False(result.Success);
        Assert.Contains(result.UnsupportedConstructs, i => i.Contains("Grace"));
    }

    [Fact]
    public void Generate_IsDeterministic_ForTheSameInput()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
              <note><pitch><step>D</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
            </measure>
            """);

        var first = MusicXmlExpectedEventGenerator.Generate(musicXml);
        var second = MusicXmlExpectedEventGenerator.Generate(musicXml);

        var firstJson = JsonSerializer.Serialize(first.Document, ContentJsonOptions.Default);
        var secondJson = JsonSerializer.Serialize(second.Document, ContentJsonOptions.Default);

        Assert.Equal(firstJson, secondJson);
    }

    [Fact]
    public void Generate_UsesTheDeclaredTempo_WhenSoundElementIsPresent()
    {
        var musicXml = Wrap($"""
            <measure number="1">
              {AttributesBlock}
              <direction><sound tempo="72"/></direction>
              <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
            </measure>
            """);

        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.True(result.Success, string.Join("; ", result.UnsupportedConstructs));
        Assert.Equal(72, result.Document!.TempoMap.Single().Bpm);
    }

    [Fact]
    public void Generate_DoesNotThrowOrResolveExternalEntities_ForAnXxeAttempt()
    {
        var musicXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <!DOCTYPE score-partwise [<!ENTITY xxe SYSTEM "file:///nonexistent">]>
            <score-partwise version="4.0">
              <part-list><score-part id="P1"><part-name>&xxe;</part-name></score-part></part-list>
              <part id="P1">
                <measure number="1">
                  <attributes><divisions>1</divisions><time><beats>4</beats><beat-type>4</beat-type></time></attributes>
                  <note><pitch><step>C</step><octave>4</octave></pitch><duration>1</duration><type>quarter</type></note>
                </measure>
              </part>
            </score-partwise>
            """;

        // DTD processing is disabled, so the undeclared entity cannot be resolved.
        // The important guarantee is that this fails gracefully (reported, not a
        // crash and not silently reading the referenced file) rather than that it succeeds.
        var result = MusicXmlExpectedEventGenerator.Generate(musicXml);

        Assert.False(result.Success);
        Assert.Null(result.Document);
        Assert.NotEmpty(result.UnsupportedConstructs);
    }
}