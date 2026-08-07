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
        new("meter", "Taktarten", "Tanda birama", "Erkenne und fühle Zweier-, Dreier- und Vierertakte.", "Kenali dan rasakan birama dua, tiga, dan empat.", "rhythm"),
        new("treble-reading", "Violinschlüssel lesen", "Membaca kunci G", "Lies Töne schrittweise ohne Buchstabieren.", "Baca nada bertahap tanpa mengeja.", "reading"),
        new("bass-reading", "Bassschlüssel lesen", "Membaca kunci F", "Finde Basstöne über Orientierungspunkte.", "Temukan nada bas melalui titik acuan.", "reading"),
        new("intervals", "Intervalle erkennen", "Mengenali interval", "Erkenne Schritte, Sprünge und Wiederholungen.", "Kenali langkah, lompatan, dan pengulangan.", "reading"),
        new("chords", "Akkorde lesen", "Membaca akor", "Erkenne einfache Dreiklänge als Form.", "Kenali triad sederhana sebagai bentuk.", "reading"),
        new("hands", "Hände getrennt", "Tangan terpisah", "Stabilisiere jede Hand einzeln.", "Stabilkan setiap tangan secara terpisah.", "technique"),
        new("coordination", "Hände koordinieren", "Koordinasi tangan", "Verbinde beide Hände in kleinen Abschnitten.", "Gabungkan kedua tangan dalam bagian kecil.", "technique"),
        new("legato", "Legato", "Legato", "Verbinde Töne ohne Lücken und ohne Verkrampfung.", "Hubungkan nada tanpa celah dan tanpa tegang.", "technique"),
        new("staccato", "Staccato", "Staccato", "Spiele kurze Töne mit lockerer Bewegung.", "Mainkan nada pendek dengan gerakan rileks.", "technique"),
        new("dynamics", "Lautstärke gestalten", "Membentuk dinamika", "Unterscheide leise, mittel und kräftig.", "Bedakan lembut, sedang, dan kuat.", "expression"),
        new("articulation", "Artikulation gestalten", "Membentuk artikulasi", "Wechsle kontrolliert zwischen gebunden und kurz.", "Beralih dengan terkontrol antara legato dan pendek.", "expression"),
        new("sustain", "Haltepedal grundlegend", "Dasar pedal sustain", "Wechsle das Haltepedal sauber ohne Klangbrei.", "Ganti pedal sustain dengan bersih tanpa suara kabur.", "pedal"),
        new("ear-direction", "Tonrichtung hören", "Mendengar arah nada", "Erkenne höher, tiefer und gleich.", "Kenali lebih tinggi, lebih rendah, dan sama.", "ear"),
        new("ear-pattern", "Muster nachspielen", "Meniru pola", "Höre kurze Muster und spiele sie nach.", "Dengarkan pola pendek lalu tirukan.", "ear"),
        new("ear-memory", "Klang im Gedächtnis", "Mengingat bunyi", "Merke dir kurze Folgen und prüfe sie am Klavier.", "Ingat urutan pendek dan periksa di piano.", "ear"),
        new("repertoire-reading", "Stück erschließen", "Mempelajari karya", "Teile ein kurzes Stück in lesbare Abschnitte.", "Bagi karya pendek menjadi bagian yang mudah dibaca.", "repertoire"),
        new("repertoire-performance", "Stück vortragen", "Membawakan karya", "Spiele ein vollständiges kurzes Stück mit stabilem Puls und Gestaltung.", "Mainkan karya pendek secara utuh dengan ketukan dan ekspresi yang stabil.", "repertoire"),
        new("practice-plan", "Wirksam üben", "Berlatih efektif", "Wähle Abschnitt, Tempo und Wiederholung bewusst.", "Pilih bagian, tempo, dan pengulangan secara sadar.", "practice"),
        new("stage-check", "Stufenprüfung", "Pemeriksaan tahap", "Verbinde Lesen, Rhythmus, Gehör und Kontrolle.", "Gabungkan membaca, ritme, pendengaran, dan kontrol.", "stage-check"),
    ];

    public static BetaCurriculum Create()
    {
        var stages = new[] { "F0", "F1", "B1", "B2", "E1" };
        var lessons = new List<BetaLessonDescriptor>(stages.Length * Topics.Length);
        var order = 1;
        foreach (var stage in stages)
        {
            foreach (var topic in Topics)
            {
                lessons.Add(new BetaLessonDescriptor(
                    $"course-{stage.ToLowerInvariant()}-{topic.Id}",
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

        var exercises = Enumerable.Range(1, 240)
            .Select(index =>
            {
                var lesson = lessons[(index - 1) % lessons.Count];
                var kind = index % 2 == 0 ? GeneratedExerciseKind.Rhythm : GeneratedExerciseKind.Reading;
                return new BetaExerciseDescriptor(
                    $"course-exercise-{index:000}",
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
        if (curriculum.Lessons.Count < 60)
        {
            issues.Add("The 1.0 catalogue must contain at least 60 guided lessons.");
        }
        if (curriculum.Exercises.Count(item => item.Kind == GeneratedExerciseKind.Rhythm) < 120)
        {
            issues.Add("The 1.0 catalogue must contain at least 120 technique/rhythm exercises.");
        }
        if (curriculum.Exercises.Count(item => item.Kind == GeneratedExerciseKind.Reading) < 80)
        {
            issues.Add("The 1.0 catalogue must contain at least 80 reading configurations.");
        }
        if (!curriculum.Lessons.Any(item => item.Stage == "E1"))
        {
            issues.Add("The 1.0 catalogue requires selected E1 lessons.");
        }
        if (curriculum.Lessons.Count(item => item.ActivityKind == "stage-check") < 5)
        {
            issues.Add("Every course stage requires an assessment.");
        }
        if (!curriculum.Lessons.Any(item => item.Id == "course-e1-stage-check"))
        {
            issues.Add("The 1.0 catalogue requires a final beginner assessment.");
        }
        if (curriculum.Lessons.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != curriculum.Lessons.Count)
        {
            issues.Add("Lesson IDs must be unique.");
        }
        if (curriculum.Exercises.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != curriculum.Exercises.Count)
        {
            issues.Add("Exercise IDs must be unique.");
        }
        if (curriculum.Lessons.Any(item => string.IsNullOrWhiteSpace(item.TitleDe) || string.IsNullOrWhiteSpace(item.TitleId) || string.IsNullOrWhiteSpace(item.GoalDe) || string.IsNullOrWhiteSpace(item.GoalId)))
        {
            issues.Add("Every lesson requires German and Indonesian title and goal text.");
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
