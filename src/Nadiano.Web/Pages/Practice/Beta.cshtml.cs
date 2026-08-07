using System.Globalization;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Core.Beta;

namespace Nadiano.Web.Pages.Practice;

public sealed class BetaModel : PageModel
{
    public IReadOnlyList<RepertoirePieceDescriptor> Repertoire { get; } = ReleaseContentCatalogue.Create().Repertoire;

    public string T(string german, string indonesian) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? indonesian : german;

    public string PieceTitle(RepertoirePieceDescriptor piece) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? piece.TitleId : piece.TitleDe;
}
