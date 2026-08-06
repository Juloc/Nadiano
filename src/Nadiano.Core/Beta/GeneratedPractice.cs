namespace Nadiano.Core.Beta;

public enum GeneratedExerciseKind
{
    Reading,
    Rhythm,
}

public sealed record GeneratedExerciseTemplate(
    string Id,
    GeneratedExerciseKind Kind,
    string SkillId,
    int MinimumMidiNote,
    int MaximumMidiNote,
    int Measures,
    int BeatsPerMeasure,
    IReadOnlyList<int> AllowedDurationUnits,
    string Clef);

public sealed record GeneratedPracticeEvent(
    int Index,
    int Measure,
    int OnsetUnits,
    int DurationUnits,
    int MidiNote);

public sealed record GeneratedPracticeCard(
    string TemplateId,
    int Seed,
    GeneratedExerciseKind Kind,
    string SkillId,
    int BeatsPerMeasure,
    int UnitsPerBeat,
    IReadOnlyList<GeneratedPracticeEvent> Events);

public static class SeededPracticeCardGenerator
{
    public const int UnitsPerBeat = 2;

    public static GeneratedPracticeCard Generate(GeneratedExerciseTemplate template, int seed)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (template.Measures < 1 || template.BeatsPerMeasure < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(template), "Measures and beats must be positive.");
        }

        if (template.MinimumMidiNote is < 0 or > 127 ||
            template.MaximumMidiNote is < 0 or > 127 ||
            template.MinimumMidiNote > template.MaximumMidiNote)
        {
            throw new ArgumentOutOfRangeException(nameof(template), "MIDI range must be between 0 and 127.");
        }

        var allowedDurations = template.AllowedDurationUnits
            .Where(duration => duration > 0)
            .Distinct()
            .OrderBy(duration => duration)
            .ToArray();

        if (allowedDurations.Length == 0 || allowedDurations[0] != 1)
        {
            throw new ArgumentException("Allowed durations must include one unit so every measure can be filled.", nameof(template));
        }

        var random = new Random(seed);
        var events = new List<GeneratedPracticeEvent>();
        var eventIndex = 0;
        var unitsPerMeasure = template.BeatsPerMeasure * UnitsPerBeat;

        for (var measure = 1; measure <= template.Measures; measure++)
        {
            var onset = 0;
            while (onset < unitsPerMeasure)
            {
                var remaining = unitsPerMeasure - onset;
                var choices = allowedDurations.Where(duration => duration <= remaining).ToArray();
                var duration = choices[random.Next(choices.Length)];
                var midiNote = template.Kind == GeneratedExerciseKind.Rhythm
                    ? 60
                    : random.Next(template.MinimumMidiNote, template.MaximumMidiNote + 1);

                events.Add(new GeneratedPracticeEvent(
                    eventIndex,
                    measure,
                    onset,
                    duration,
                    midiNote));

                eventIndex++;
                onset += duration;
            }
        }

        return new GeneratedPracticeCard(
            template.Id,
            seed,
            template.Kind,
            template.SkillId,
            template.BeatsPerMeasure,
            UnitsPerBeat,
            events);
    }
}
