using System.Text;

namespace Nadiano.Core.Beta;

public sealed record EarTrainingTaskDescriptor(
    string Id,
    string SkillId,
    string Kind,
    int Seed);

public sealed record RepertoirePieceDescriptor(
    string Id,
    string Stage,
    string TitleDe,
    string TitleId,
    string Composer,
    string SourceKind,
    string Attribution,
    string SourceReference,
    int TargetTempoBpm,
    IReadOnlyList<int> MidiNotes);

public sealed record ReleaseContentInventory(
    IReadOnlyList<EarTrainingTaskDescriptor> EarTasks,
    IReadOnlyList<RepertoirePieceDescriptor> Repertoire);

public static class ReleaseContentCatalogue
{
    public const string OriginalSourceKind = "original";
    public const string PublicDomainSourceKind = "public-domain";

    private static readonly int[][] OriginalPatterns =
    [
        [0, 2, 4, 5, 4, 2, 0, 2, 4, 5, 7, 5, 4, 2, 0, 0],
        [0, 4, 2, 5, 4, 7, 5, 4, 2, 0, 2, 4, 5, 4, 2, 0],
        [0, 2, 4, 7, 5, 4, 2, 0, 4, 5, 7, 9, 7, 5, 4, 2],
        [0, 5, 4, 2, 0, 2, 4, 5, 7, 4, 5, 2, 4, 2, 0, 0],
        [0, 2, 5, 4, 7, 5, 9, 7, 5, 4, 2, 4, 5, 2, 0, 0],
        [0, 4, 7, 4, 5, 9, 7, 5, 4, 2, 0, 2, 4, 5, 2, 0],
    ];

    public static ReleaseContentInventory Create()
    {
        var earTasks = Enumerable.Range(1, 60)
            .Select(index =>
            {
                var kind = index % 3 switch
                {
                    1 => "direction",
                    2 => "pattern",
                    _ => "memory",
                };
                return new EarTrainingTaskDescriptor(
                    $"ear-task-{index:000}",
                    $"ear.{kind}",
                    kind,
                    30_000 + index);
            })
            .ToArray();

        var repertoire = CreateOriginalPieces()
            .Concat(CreatePublicDomainPieces())
            .ToArray();

        return new ReleaseContentInventory(earTasks, repertoire);
    }

    public static RepertoirePieceDescriptor? FindRepertoire(string id) =>
        Create().Repertoire.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));

    public static IReadOnlyList<string> Validate(ReleaseContentInventory inventory)
    {
        var issues = new List<string>();
        if (inventory.EarTasks.Count < 60)
        {
            issues.Add("The 1.0 catalogue must contain at least 60 ear-training tasks.");
        }
        if (inventory.Repertoire.Count(item => item.SourceKind == OriginalSourceKind) < 24)
        {
            issues.Add("The 1.0 catalogue must contain at least 24 original mini-pieces.");
        }
        if (inventory.Repertoire.Count(item => item.SourceKind == PublicDomainSourceKind) < 12)
        {
            issues.Add("The 1.0 catalogue must contain at least 12 verified public-domain melodies in Nadiano editions.");
        }
        if (inventory.EarTasks.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != inventory.EarTasks.Count)
        {
            issues.Add("Ear-training task IDs must be unique.");
        }
        if (inventory.Repertoire.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != inventory.Repertoire.Count)
        {
            issues.Add("Repertoire IDs must be unique.");
        }
        if (inventory.Repertoire.Any(item => item.MidiNotes.Count < 8 || item.MidiNotes.Any(note => note is < 21 or > 108)))
        {
            issues.Add("Every repertoire item requires a playable piano-range study of at least eight notes.");
        }
        if (inventory.Repertoire.Any(item => string.IsNullOrWhiteSpace(item.TitleDe) || string.IsNullOrWhiteSpace(item.TitleId) || string.IsNullOrWhiteSpace(item.Attribution)))
        {
            issues.Add("Every repertoire item requires bilingual titles and attribution.");
        }
        if (inventory.Repertoire.Where(item => item.SourceKind == PublicDomainSourceKind).Any(item => string.IsNullOrWhiteSpace(item.Composer) || string.IsNullOrWhiteSpace(item.SourceReference)))
        {
            issues.Add("Every public-domain repertoire item requires composer and source-reference records.");
        }
        return issues;
    }

    public static string CreateMusicXml(RepertoirePieceDescriptor piece)
    {
        var builder = new StringBuilder();
        builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        builder.Append("<score-partwise version=\"3.1\">");
        builder.Append("<work><work-title>").Append(XmlText(piece.TitleDe)).Append("</work-title></work>");
        builder.Append("<identification><creator type=\"composer\">").Append(XmlText(piece.Composer)).Append("</creator></identification>");
        builder.Append("<part-list><score-part id=\"P1\"><part-name>Piano</part-name></score-part></part-list><part id=\"P1\">");

        for (var index = 0; index < piece.MidiNotes.Count; index++)
        {
            if (index % 4 == 0)
            {
                if (index > 0)
                {
                    builder.Append("</measure>");
                }
                builder.Append("<measure number=\"").Append(index / 4 + 1).Append("\">");
                if (index == 0)
                {
                    builder.Append("<attributes><divisions>1</divisions><key><fifths>0</fifths></key><time><beats>4</beats><beat-type>4</beat-type></time><clef><sign>G</sign><line>2</line></clef></attributes>");
                    builder.Append("<direction placement=\"above\"><direction-type><metronome><beat-unit>quarter</beat-unit><per-minute>")
                        .Append(piece.TargetTempoBpm)
                        .Append("</per-minute></metronome></direction-type><sound tempo=\"")
                        .Append(piece.TargetTempoBpm)
                        .Append("\"/></direction>");
                }
            }

            var pitch = ToPitch(piece.MidiNotes[index]);
            builder.Append("<note><pitch><step>").Append(pitch.Step).Append("</step>");
            if (pitch.Alter != 0)
            {
                builder.Append("<alter>").Append(pitch.Alter).Append("</alter>");
            }
            builder.Append("<octave>").Append(pitch.Octave).Append("</octave></pitch><duration>1</duration><voice>1</voice><type>quarter</type></note>");
        }

        if (piece.MidiNotes.Count > 0)
        {
            builder.Append("</measure>");
        }
        builder.Append("</part></score-partwise>");
        return builder.ToString();
    }

    private static IEnumerable<RepertoirePieceDescriptor> CreateOriginalPieces()
    {
        var transpositions = new[] { 0, 2, 5, 7 };
        for (var index = 1; index <= 24; index++)
        {
            var pattern = OriginalPatterns[(index - 1) % OriginalPatterns.Length];
            var transposition = transpositions[(index - 1) / OriginalPatterns.Length];
            var notes = pattern.Select(offset => 60 + transposition + offset).ToArray();
            var stage = index <= 6 ? "F0" : index <= 12 ? "F1" : index <= 18 ? "B1" : "B2";
            yield return new RepertoirePieceDescriptor(
                $"nadiano-mini-{index:00}",
                stage,
                $"Nadiano Miniatur {index:00}",
                $"Miniatur Nadiano {index:00}",
                "Nadiano",
                OriginalSourceKind,
                "Original composition created for Nadiano; no external score or arrangement used.",
                "Nadiano original composition inventory",
                60 + index % 6 * 4,
                notes);
        }
    }

    private static IEnumerable<RepertoirePieceDescriptor> CreatePublicDomainPieces()
    {
        yield return PublicDomain("pd-ode-to-joy", "Ode an die Freude", "Ode to Joy", "Ludwig van Beethoven", "Symphony No.9, Op.125", 76, [64, 64, 65, 67, 67, 65, 64, 62, 60, 60, 62, 64, 64, 62, 62, 64]);
        yield return PublicDomain("pd-symphony-5", "Fünfte Sinfonie – Motiv", "Symphony No.5 – Motif", "Ludwig van Beethoven", "Symphony No.5, Op.67", 72, [67, 67, 67, 63, 65, 65, 65, 62, 67, 67, 67, 63, 68, 68, 68, 67]);
        yield return PublicDomain("pd-fur-elise", "Für Elise – Motiv", "Für Elise – Motif", "Ludwig van Beethoven", "Für Elise, WoO 59", 72, [76, 75, 76, 75, 76, 71, 74, 72, 69, 60, 64, 69, 71, 64, 68, 71]);
        yield return PublicDomain("pd-eine-kleine-nachtmusik", "Eine kleine Nachtmusik – Motiv", "Eine kleine Nachtmusik – Motif", "Wolfgang Amadeus Mozart", "Serenade No.13, K.525", 88, [67, 62, 67, 71, 74, 71, 67, 71, 67, 62, 67, 71, 74, 71, 67, 62]);
        yield return PublicDomain("pd-bach-prelude-c", "Präludium C-Dur – Studie", "C Major Prelude – Study", "Johann Sebastian Bach", "Prelude and Fugue in C major, BWV 846", 68, [60, 64, 67, 72, 64, 67, 72, 76, 60, 65, 69, 72, 65, 69, 72, 77]);
        yield return PublicDomain("pd-minuet-g", "Menuett G-Dur – Motiv", "Minuet in G Major – Motif", "Christian Petzold", "Minuet in G major, BWV Anh.114", 84, [67, 64, 65, 67, 69, 71, 72, 67, 64, 60, 62, 64, 65, 62, 60, 59]);
        yield return PublicDomain("pd-brahms-lullaby", "Wiegenlied – Motiv", "Lullaby – Motif", "Johannes Brahms", "Wiegenlied, Op.49 No.4", 64, [64, 64, 67, 64, 64, 67, 64, 67, 72, 71, 69, 69, 67, 62, 64, 65]);
        yield return PublicDomain("pd-new-world", "Aus der Neuen Welt – Largo", "New World – Largo", "Antonín Dvořák", "Symphony No.9, Op.95", 66, [64, 67, 67, 64, 62, 60, 62, 64, 67, 64, 62, 60, 59, 60, 62, 64]);
        yield return PublicDomain("pd-morning-mood", "Morgenstimmung – Motiv", "Morning Mood – Motif", "Edvard Grieg", "Peer Gynt Suite No.1, Op.46", 70, [67, 64, 62, 60, 62, 64, 67, 64, 67, 69, 64, 69, 67, 64, 62, 60]);
        yield return PublicDomain("pd-blue-danube", "An der schönen blauen Donau – Motiv", "Blue Danube – Motif", "Johann Strauss II", "An der schönen blauen Donau, Op.314", 86, [67, 71, 74, 74, 76, 71, 71, 67, 67, 71, 74, 74, 76, 71, 71, 67]);
        yield return PublicDomain("pd-can-can", "Can-Can – Motiv", "Can-Can – Motif", "Jacques Offenbach", "Orphée aux enfers", 96, [72, 72, 74, 76, 77, 76, 74, 72, 71, 71, 72, 74, 76, 74, 72, 71]);
        yield return PublicDomain("pd-the-swan", "Der Schwan – Motiv", "The Swan – Motif", "Camille Saint-Saëns", "Le carnaval des animaux: Le cygne", 62, [67, 69, 71, 72, 74, 72, 71, 69, 67, 64, 65, 67, 69, 67, 65, 64]);
    }

    private static RepertoirePieceDescriptor PublicDomain(
        string id,
        string titleDe,
        string titleId,
        string composer,
        string sourceReference,
        int tempo,
        IReadOnlyList<int> notes) =>
        new(
            id,
            "B2",
            titleDe,
            titleId,
            composer,
            PublicDomainSourceKind,
            $"Public-domain composition by {composer}; simplified Nadiano study edition created independently for this project.",
            $"IMSLP work record: {sourceReference}",
            tempo,
            notes);

    private static (string Step, int Alter, int Octave) ToPitch(int midiNote)
    {
        var pitchClass = midiNote % 12;
        var octave = midiNote / 12 - 1;
        return pitchClass switch
        {
            0 => ("C", 0, octave),
            1 => ("C", 1, octave),
            2 => ("D", 0, octave),
            3 => ("E", -1, octave),
            4 => ("E", 0, octave),
            5 => ("F", 0, octave),
            6 => ("F", 1, octave),
            7 => ("G", 0, octave),
            8 => ("A", -1, octave),
            9 => ("A", 0, octave),
            10 => ("B", -1, octave),
            _ => ("B", 0, octave),
        };
    }

    private static string XmlText(string value) =>
        value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
}
