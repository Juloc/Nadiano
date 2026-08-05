using System.Globalization;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Web.Features.Progress;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Progress;

public class IndexModel(
    ProgressSummaryService progress,
    CurrentProfileAccessor profiles) : PageModel
{
    public ProgressSummary Summary { get; private set; } = new(
        0,
        0,
        [],
        [],
        [],
        [],
        [],
        null,
        null);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        Summary = await progress.BuildAsync(
            profileId,
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            DateTimeOffset.UtcNow,
            cancellationToken);
    }

    public static string NextActionResourceKey(string code) => code switch
    {
        "hands-separate" => "Practice.NextAction.HandsSeparate",
        "repeat-slower" => "Practice.NextAction.RepeatSlower",
        "repeat-section" => "Practice.NextAction.RepeatSection",
        "well-done" => "Practice.NextAction.WellDone",
        _ => "Progress.Recommendation.Continue",
    };
}
