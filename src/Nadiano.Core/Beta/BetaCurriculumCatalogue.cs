namespace Nadiano.Core.Beta;

public sealed record BetaLessonDescriptor(
    string Id,
    string Stage,
    int Order,
    string SkillId,
    string TitleDe,
    string TitleId,
    string GoalDe,
    string GoalId,
    string ActivityKind);

public sealed record BetaExerciseDescriptor(
    string Id,
    string LessonId,
    string SkillId,
    GeneratedExerciseKind Kind,
    int Seed);

public sealed record BetaCurriculum(
    IReadOnlyList<BetaLessonDescriptor> Lessons,
    IReadOnlyList<BetaExerciseDescriptor> Exercises);

public static class BetaCurriculumCatalogue
{
    private static readonly Topic[] Topics =
    [
        new("pulse", "Puls halten", "Menjaga ketukan", "Halte einen ruhigen Grundpuls.", "Pertahankan ketukan dasar yang stabil.", "rhythm"),
        new("subdivision", "Unterteilungen", "Pembagian ketukan", "Zähle und spiele Achtel sicher.", "Hitung dan mainkan not seperdelapan dengan stabil.", "rhythm"),
        new("rests", "Pausen lesen", "Membaca tanda istirahat", "Halte Pausen exakt aus.", "Pertahankan durasi istirahat dengan tepat.", "rhythm"),
        new("treble-reading", "Violinschlüssel lesen", "Membaca kunci G", "Lies Töne schrittweise ohne Buchstabieren.", "Baca nada bertahap tanpa mengeja.", "reading"),
        new("bass-reading", "Bassschlüssel lesen", "Membaca kunci F", "Finde Basstöne über Orientierungspunkte.", "Temukan nada bas melalui titik acuan.", "reading"),
        new("intervals", "Intervalle erkennen", "Mengenali interval", "Erkenne Schritte, Sprünge und Wiederholungen.", "Kenali langkah, lompatan, dan pengulangan.", "reading"),
        new("hands", "Hände getrennt", "Tangan terpisah", "Stabilisiere jede Hand einzeln.", "Stabilkan setiap tangan secara terpisah.", "technique"),
        new("coordination", "Hände koordinieren", "Koordinasi tangan", "Verbinde beide Hände in kleinen Abschnitten.", "Gabungkan kedua tangan dalam bagian kecil.", "technique"),
        new("legato", "Legato", "Legato", "Verbinde Töne ohne Lücken und ohne Verkrampfung.", "Hubungkan nada tanpa celah dan tanpa tegang.", "technique"),
        new("staccato", "Staccato", "Staccato", "Spiele kurze Töne mit lockerer Bewegung.", "Mainkan nada pendek dengan gerakan rileks.", "technique"),
        new("dynamics", "Lautstärke gestalten", "Membentuk dinamika", "Unterscheide leise, mittel und kräftig.", "Bedakan lembut, sedang, dan kuat.", "expression"),
        new("ear-direction", "Tonrichtung hören", "Mendengar arah nada", "Erkenne höher, tiefer und gleich.", "Kenali lebih tinggi, lebih rendah, dan sama.", "ear"),
        new("ear-pattern", "Muster nachspielen", "Meniru pola", "Höre kurze Muster und spiele sie nach.", "Dengarkan pola pendek lalu tirukan.", "ear"),
        new("practice-plan", "Wirksam üben", "Berlatih efektif", "Wähle Abschnitt, Tempo und Wiederholung bewusst.", "Pilih bagian, tempo, dan pengulangan secara sadar.", "practice"),
        new("stage-check", "Stufenprüfung", "Pemeriksaan tahap", "Verbinde Lesen, Rhythmus und Kontrolle.", "Gabungkan membaca, ritme, dan kontrol.", "stage-check"),
    ];

    public static BetaCurriculum Create()
    {
        var lessons = new List<BetaLessonDescriptor>(45);
        var order = 1;
        foreach (var stage in new[] { "B1", "B2", "B2+" })
        {
            foreach (var topic in Topics)
            {
                lessons.Add(new BetaLessonDescriptor(
                    $"beta-{stage.ToLowerInvariant().Replace('+', 'p')}-{topic.Id}",
                    stage,
                    order++,
                    $"{topic.ActivityKind}.{topic.Id}",
                    $"{stage}: {topic.TitleDe}",
                    $"{stage}: {topic.TitleId}",
                    topic.GoalDe,
                    topic.GoalId,
                    topic.ActivityKind));
            }
        }

        var exercises = Enumerable.Range(1, 100)
            .Select(index =>
            {
                var lesson = lessons[(index - 1) % lessons.Count];
                var kind = index % 2 == 0 ? GeneratedExerciseKind.Rhythm : GeneratedExerciseKind.Reading;
                return new BetaExerciseDescriptor(
                    $"beta-exercise-{index:000}",
                    lesson.Id,
                    lesson.SkillId,
                    kind,
                    10_000 + index);
            })
            .ToArray();

        return new BetaCurriculum(lessons, exercises);
    }

    public static IReadOnlyList<string> Validate(BetaCurriculum curriculum)
    {
        var issues = new List<string>();
        if (curriculum.Lessons.Count < 45)
        {
            issues.Add("Beta catalogue must contain at least 45 guided lessons.");
        }
        if (curriculum.Exercises.Count < 100)
        {
            issues.Add("Beta catalogue must contain at least 100 exercises.");
        }
        if (curriculum.Lessons.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != curriculum.Lessons.Count)
        {
            issues.Add("Beta lesson IDs must be unique.");
        }
        if (curriculum.Exercises.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != curriculum.Exercises.Count)
        {
            issues.Add("Beta exercise IDs must be unique.");
        }
        if (curriculum.Lessons.Any(item => string.IsNullOrWhiteSpace(item.TitleDe) || string.IsNullOrWhiteSpace(item.TitleId) || string.IsNullOrWhiteSpace(item.GoalDe) || string.IsNullOrWhiteSpace(item.GoalId)))
        {
            issues.Add("Every Beta lesson requires German and Indonesian title and goal text.");
        }
        return issues;
    }

    private sealed record Topic(
        string Id,
        string TitleDe,
        string TitleId,
        string GoalDe,
        string GoalId,
        string ActivityKind);
}
