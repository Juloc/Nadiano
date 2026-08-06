using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Web.Features.Beta;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Learn;

public sealed class BetaModel(
    BetaCourseProgressService courseProgress,
    CurrentProfileAccessor profiles) : PageModel
{
    public IReadOnlyList<BetaCourseMapItem> Items { get; private set; } = [];
    public string? NoticeCode { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCompleteAsync(string lessonId, CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        var result = await courseProgress.CompleteAsync(profileId, lessonId, cancellationToken);
        NoticeCode = result.Code;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public string Text(string german, string indonesian) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? indonesian : german;

    public string Title(BetaCourseMapItem item) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? item.Lesson.TitleId : item.Lesson.TitleDe;

    public string Goal(BetaCourseMapItem item) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? item.Lesson.GoalId : item.Lesson.GoalDe;

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        Items = await courseProgress.GetMapAsync(profileId, cancellationToken);
    }
}