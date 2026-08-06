using System.Globalization;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nadiano.Web.Pages.Practice;

public sealed class BetaModel : PageModel
{
    public string T(string german, string indonesian) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? indonesian : german;
}
