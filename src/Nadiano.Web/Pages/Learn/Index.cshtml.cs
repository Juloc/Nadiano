using System.Globalization;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Core.Content;
using Nadiano.Core.Courses;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Learn;

public class IndexModel(
    ContentCatalogue catalogue,
    CourseProgressService progress,
    BundledContentRepository content,
    CurrentProfileAccessor profiles) : PageModel
{
    public IReadOnlyList<CourseView> Courses { get; private set; } = [];

    public sealed record LessonView(
        string LessonId,
        string Title,
        LessonAvailability Availability,
        IReadOnlyList<string> MissingPrerequisiteTitles);

    public sealed record StageView(string StageId, string TitleKey, IReadOnlyList<LessonView> Lessons);

    public sealed record CourseView(string CourseId, IReadOnlyList<StageView> Stages, string? RecommendedLessonTitle);

    public async Task OnGetAsync()
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext);
        var culture = CultureInfo.CurrentUICulture.Name;

        var courses = new List<CourseView>();
        foreach (var courseId in catalogue.CourseIds)
        {
            var map = await progress.GetCourseMapAsync(profileId, courseId);
            if (map is null)
            {
                continue;
            }

            string TitleOf(string lessonId) => content.LoadLocalizedText(courseId, lessonId, culture).Title;

            var stages = map.Course.Stages
                .Select(stage => new StageView(
                    stage.Id,
                    stage.TitleKey,
                    map.Entries
                        .Where(entry => entry.StageId == stage.Id)
                        .Select(entry => new LessonView(
                            entry.LessonId,
                            TitleOf(entry.LessonId),
                            entry.Availability,
                            entry.MissingPrerequisites.Select(TitleOf).ToArray()))
                        .ToArray()))
                .Where(stage => stage.Lessons.Count > 0)
                .ToArray();

            var recommendedTitle = map.RecommendedNextLessonId is null ? null : TitleOf(map.RecommendedNextLessonId);

            courses.Add(new CourseView(courseId, stages, recommendedTitle));
        }

        Courses = courses;
    }
}
