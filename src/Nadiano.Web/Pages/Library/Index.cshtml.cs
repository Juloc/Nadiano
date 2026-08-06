using System.Globalization;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Nadiano.Core.Beta;
using Nadiano.Web.Features.Library;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Library;

public sealed class IndexModel(
    PrivateLibraryService library,
    CurrentProfileAccessor profiles) : PageModel
{
    public IReadOnlyList<PrivateLibraryItem> Items { get; private set; } = [];
    public string? Notice { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostImportAsync(
        IFormFile? upload,
        string? title,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        if (upload is null)
        {
            Error = T("Bitte eine MusicXML- oder MXL-Datei auswählen.", "Pilih berkas MusicXML atau MXL.");
            await LoadAsync(cancellationToken);
            return Page();
        }

        var result = await library.ImportAsync(profileId, upload, title, cancellationToken);
        if (!result.Success)
        {
            Error = result.ErrorCode switch
            {
                "invalid-size" => T("Die Datei ist leer oder größer als 8 MB.", "Berkas kosong atau lebih besar dari 8 MB."),
                "invalid-type" => T("Erlaubt sind .musicxml, .xml und .mxl.", "Format yang diizinkan: .musicxml, .xml, dan .mxl."),
                "not-score-partwise" => T("Nur score-partwise MusicXML wird unterstützt.", "Hanya MusicXML score-partwise yang didukung."),
                _ => T("Die Datei konnte nicht sicher importiert werden.", "Berkas tidak dapat diimpor dengan aman."),
            };
        }
        else
        {
            Notice = result.Warnings.Count == 0
                ? T("Import abgeschlossen.", "Impor selesai.")
                : T("Import abgeschlossen. Nicht unterstützte Notation wurde als Warnung markiert.", "Impor selesai. Notasi yang belum didukung ditandai sebagai peringatan.");
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        Guid itemId,
        string? title,
        int fromMeasure,
        int toMeasure,
        int targetTempoBpm,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        await library.UpdateAsync(profileId, itemId, title, fromMeasure, toMeasure, targetTempoBpm, cancellationToken);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        await library.DeleteAsync(profileId, itemId, cancellationToken);
        return RedirectToPage();
    }

    public LibraryItemMetadata Metadata(PrivateLibraryItem item) =>
        JsonSerializer.Deserialize<LibraryItemMetadata>(item.MetadataJson)
        ?? new LibraryItemMetadata(1, 1, 0, 1, 1, 90);

    public IReadOnlyList<string> Warnings(PrivateLibraryItem item) =>
        JsonSerializer.Deserialize<string[]>(item.WarningJson) ?? [];

    public string T(string german, string indonesian) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "id" ? indonesian : german;

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(HttpContext, cancellationToken);
        Items = await library.ListAsync(profileId, cancellationToken);
    }
}
