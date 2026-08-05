using System.Xml;
using System.Xml.Linq;

using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Content;

/// <summary>
/// Generates a deterministic <see cref="ExpectedEventDocument"/> from a
/// limited, explicitly supported MusicXML subset (docs/CONTENT_MODEL.md §7,
/// docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-013): single part per call, a
/// shared time cursor per measure driven by note/backup/forward, chords,
/// ties and 4-denominator time signatures. Constructs outside that subset
/// are reported in <see cref="ExpectedEventGenerationResult.UnsupportedConstructs"/>
/// rather than guessed.
/// </summary>
public static class MusicXmlExpectedEventGenerator
{
    private static readonly Dictionary<string, int> BaseSemitone = new()
    {
        ["C"] = 0,
        ["D"] = 2,
        ["E"] = 4,
        ["F"] = 5,
        ["G"] = 7,
        ["A"] = 9,
        ["B"] = 11,
    };

    public static ExpectedEventGenerationResult Generate(
        string musicXml,
        IReadOnlyDictionary<string, Hand>? partHandMapping = null,
        int defaultTempoBpm = 90)
    {
        var unsupported = new List<string>();
        XDocument document;

        try
        {
            document = LoadSecurely(musicXml);
        }
        catch (Exception ex) when (ex is XmlException or InvalidOperationException)
        {
            return new ExpectedEventGenerationResult { UnsupportedConstructs = [$"MusicXML could not be parsed: {ex.Message}"] };
        }

        var root = document.Root;
        if (root is null || root.Name.LocalName != "score-partwise")
        {
            return new ExpectedEventGenerationResult { UnsupportedConstructs = ["Only score-partwise MusicXML is supported."] };
        }

        var tempoMap = ReadTempoMap(root, defaultTempoBpm);
        var events = new List<ExpectedEvent>();

        foreach (var partElement in root.Elements("part"))
        {
            var partId = partElement.Attribute("id")?.Value ?? "";
            var hand = partHandMapping is not null && partHandMapping.TryGetValue(partId, out var mappedHand) ? mappedHand : (Hand?)null;
            ParsePart(partElement, hand, events, unsupported);
        }

        if (unsupported.Count > 0)
        {
            return new ExpectedEventGenerationResult { UnsupportedConstructs = unsupported };
        }

        var document_ = new ExpectedEventDocument
        {
            SchemaVersion = 1,
            TimeBase = "beats",
            TempoMap = tempoMap,
            Events = events,
        };

        return new ExpectedEventGenerationResult { Document = document_ };
    }

    private static XDocument LoadSecurely(string musicXml)
    {
        // Disables DTD/external-entity resolution (XXE hardening, docs/TECHNICAL_ARCHITECTURE.md §19)
        // while still tolerating the MusicXML DOCTYPE declaration itself.
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,
        };

        using var stringReader = new StringReader(musicXml);
        using var xmlReader = XmlReader.Create(stringReader, settings);
        return XDocument.Load(xmlReader);
    }

    private static IReadOnlyList<TempoMapEntry> ReadTempoMap(XElement root, int defaultTempoBpm)
    {
        var soundTempo = root
            .Descendants("sound")
            .Select(s => s.Attribute("tempo")?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        var bpm = soundTempo is not null && double.TryParse(soundTempo, out var parsedTempo)
            ? (int)Math.Round(parsedTempo)
            : Math.Max(1, defaultTempoBpm);

        return [new TempoMapEntry { Beat = 0, Bpm = bpm }];
    }

    private static void ParsePart(XElement partElement, Hand? hand, List<ExpectedEvent> events, List<string> unsupported)
    {
        var divisions = 1;
        var beatType = 4;
        // A chord's non-first notes (<chord/>) share the base note's start position, so the base
        // note's own duration cannot be applied to the cursor immediately — it is queued here and
        // only folded in once the next non-chord element for that voice is reached.
        var voiceCursors = new Dictionary<string, double>();
        var pendingAdvance = new Dictionary<string, double>();
        var pendingTies = new Dictionary<string, PendingTiedGroup>();
        var eventIndexByMeasureVoice = new Dictionary<(int measure, string voice), int>();

        var measureNumber = 0;
        foreach (var measureElement in partElement.Elements("measure"))
        {
            measureNumber++;
            voiceCursors.Clear();
            pendingAdvance.Clear();

            foreach (var child in measureElement.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "attributes":
                        ReadAttributes(child, ref divisions, ref beatType, unsupported);
                        break;

                    case "backup":
                        FlushAllPending(voiceCursors, pendingAdvance);
                        AdjustAllCursors(voiceCursors, -ReadDuration(child), divisions);
                        break;

                    case "forward":
                        FlushAllPending(voiceCursors, pendingAdvance);
                        AdjustAllCursors(voiceCursors, ReadDuration(child), divisions);
                        break;

                    case "note":
                        ParseNote(
                            child,
                            measureNumber,
                            divisions,
                            beatType,
                            hand,
                            voiceCursors,
                            pendingAdvance,
                            pendingTies,
                            eventIndexByMeasureVoice,
                            events,
                            unsupported);
                        break;
                }
            }
        }

        foreach (var stillPending in pendingTies.Values)
        {
            unsupported.Add($"Unresolved tie starting at measure {stillPending.Event.Measure}, voice {stillPending.Event.Voice}.");
        }
    }

    private static void FlushAllPending(Dictionary<string, double> voiceCursors, Dictionary<string, double> pendingAdvance)
    {
        foreach (var voice in pendingAdvance.Keys.ToArray())
        {
            voiceCursors[voice] = voiceCursors.GetValueOrDefault(voice) + pendingAdvance[voice];
            pendingAdvance[voice] = 0;
        }
    }

    private static void ReadAttributes(XElement attributesElement, ref int divisions, ref int beatType, List<string> unsupported)
    {
        var divisionsText = attributesElement.Element("divisions")?.Value;
        if (divisionsText is not null && int.TryParse(divisionsText, out var parsedDivisions) && parsedDivisions > 0)
        {
            divisions = parsedDivisions;
        }

        var timeElement = attributesElement.Element("time");
        if (timeElement is not null)
        {
            var beatTypeText = timeElement.Element("beat-type")?.Value;
            if (beatTypeText is not null && int.TryParse(beatTypeText, out var parsedBeatType))
            {
                beatType = parsedBeatType;
            }
        }

        if (beatType != 4)
        {
            unsupported.Add($"Time signatures with beat-type {beatType} are not supported; only /4 meters are.");
        }

        if (attributesElement.Element("staves") is not null)
        {
            unsupported.Add("Multi-staff parts (grand staff single-part encoding) are not supported.");
        }
    }

    private static void AdjustAllCursors(Dictionary<string, double> voiceCursors, int deltaXmlUnits, int divisions)
    {
        var deltaBeats = (double)deltaXmlUnits / divisions;
        foreach (var voice in voiceCursors.Keys.ToArray())
        {
            voiceCursors[voice] += deltaBeats;
        }
    }

    private static int ReadDuration(XElement element) =>
        int.TryParse(element.Element("duration")?.Value, out var duration) ? duration : 0;

    private static void ParseNote(
        XElement noteElement,
        int measureNumber,
        int divisions,
        int beatType,
        Hand? hand,
        Dictionary<string, double> voiceCursors,
        Dictionary<string, double> pendingAdvance,
        Dictionary<string, PendingTiedGroup> pendingTies,
        Dictionary<(int measure, string voice), int> eventIndexByMeasureVoice,
        List<ExpectedEvent> events,
        List<string> unsupported)
    {
        var voice = noteElement.Element("voice")?.Value ?? "1";
        var durationXml = ReadDuration(noteElement);
        var durationBeats = (double)durationXml / divisions;
        var isChordContinuation = noteElement.Element("chord") is not null;
        var isRest = noteElement.Element("rest") is not null;

        if (noteElement.Element("time-modification") is not null)
        {
            unsupported.Add($"Tuplets (time-modification) at measure {measureNumber}, voice {voice} are not supported.");
            return;
        }

        if (noteElement.Element("grace") is not null)
        {
            unsupported.Add($"Grace notes at measure {measureNumber}, voice {voice} are not supported.");
            return;
        }

        voiceCursors.TryAdd(voice, 0);
        pendingAdvance.TryAdd(voice, 0);

        // A chord's <chord/> notes must land on the same start position as their base note, so
        // only a non-chord element folds in the PREVIOUS group's queued advance before reading
        // the cursor. The current element's own duration is queued (not applied) either way.
        if (!isChordContinuation)
        {
            voiceCursors[voice] += pendingAdvance[voice];
            pendingAdvance[voice] = 0;
        }

        var startBeat = voiceCursors[voice];

        if (isRest)
        {
            pendingAdvance[voice] = durationBeats;
            return;
        }

        var pitchElement = noteElement.Element("pitch");
        if (pitchElement is null)
        {
            unsupported.Add($"Unpitched note at measure {measureNumber}, voice {voice} is not supported.");
            return;
        }

        var midiPitch = ReadMidiPitch(pitchElement);
        var fingering = ReadFingering(noteElement);
        var tieStart = noteElement.Elements("tie").Any(t => t.Attribute("type")?.Value == "start");
        var tieStop = noteElement.Elements("tie").Any(t => t.Attribute("type")?.Value == "stop");

        if (isChordContinuation && (tieStart || tieStop))
        {
            unsupported.Add($"A tied note inside a chord at measure {measureNumber}, voice {voice} is not supported.");
            return;
        }

        var tieKey = $"{voice}|{midiPitch}";

        if (tieStop && pendingTies.TryGetValue(tieKey, out var pending))
        {
            pending.Event = pending.Event with { DurationBeats = pending.Event.DurationBeats + durationBeats };
            pendingTies.Remove(tieKey);

            if (tieStart)
            {
                pendingTies[tieKey] = pending;
            }
            else
            {
                events.Add(pending.Event);
            }

            pendingAdvance[voice] = durationBeats;
            return;
        }

        if (isChordContinuation)
        {
            var lastIndex = events.FindLastIndex(e => e.Measure == measureNumber && e.Voice == voice && Math.Abs(e.Beat - startBeat) < 1e-9);
            if (lastIndex < 0)
            {
                unsupported.Add($"Chord note at measure {measureNumber}, voice {voice}, beat {startBeat} has no preceding chord base note to merge into.");
                return;
            }

            var existing = events[lastIndex];
            var mergedPitches = existing.Pitches.Append(midiPitch).ToArray();
            var mergedFingering = existing.Fingering.Count > 0 && fingering.HasValue
                ? existing.Fingering.Append(fingering.Value).ToArray()
                : [];
            events[lastIndex] = existing with { Pitches = mergedPitches, Fingering = mergedFingering };
            return;
        }

        var key = (measureNumber, voice);
        eventIndexByMeasureVoice.TryAdd(key, 0);
        eventIndexByMeasureVoice[key]++;

        var newEvent = new ExpectedEvent
        {
            Id = $"m{measureNumber}-v{voice}-n{eventIndexByMeasureVoice[key]}",
            Measure = measureNumber,
            Beat = startBeat,
            DurationBeats = durationBeats,
            Pitches = [midiPitch],
            Hand = hand,
            Voice = voice,
            Fingering = fingering.HasValue ? [fingering.Value] : [],
        };

        if (tieStart)
        {
            pendingTies[tieKey] = new PendingTiedGroup { Event = newEvent };
        }
        else
        {
            events.Add(newEvent);
        }

        pendingAdvance[voice] = durationBeats;
    }

    private static int ReadMidiPitch(XElement pitchElement)
    {
        var step = pitchElement.Element("step")?.Value ?? "C";
        var octave = int.TryParse(pitchElement.Element("octave")?.Value, out var parsedOctave) ? parsedOctave : 4;
        var alter = int.TryParse(pitchElement.Element("alter")?.Value, out var parsedAlter) ? parsedAlter : 0;

        return ((octave + 1) * 12) + BaseSemitone[step] + alter;
    }

    private static int? ReadFingering(XElement noteElement)
    {
        var fingeringText = noteElement
            .Element("notations")?
            .Element("technical")?
            .Element("fingering")?
            .Value;

        return int.TryParse(fingeringText, out var fingering) ? fingering : null;
    }

    private sealed class PendingTiedGroup
    {
        public required ExpectedEvent Event { get; set; }
    }
}