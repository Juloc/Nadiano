using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Profiles;
using Nadiano.Web.Infrastructure.Localization;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Profiles;

public class EditModel(NadianoDbContext db, CurrentProfileAccessor profiles) : PageModel
{
    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Language { get; set; } = SupportedCultures.Default;

    [BindProperty]
    public string NoteNameSystem { get; set; } = NoteNameSystems.German;

    [BindProperty]
    public int SessionLengthMinutes { get; set; } = 20;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var profile = await db.LearnerProfiles.FindAsync(id);
        if (profile is null)
        {
            return NotFound();
        }

        var preferences = await db.ProfilePreferences.FindAsync(id);

        Id = profile.Id;
        Name = profile.Name;
        Language = preferences?.Language ?? SupportedCultures.Default;
        NoteNameSystem = preferences?.NoteNameSystem ?? NoteNameSystems.German;
        SessionLengthMinutes = preferences?.SessionLengthMinutes ?? 20;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var profile = await db.LearnerProfiles.FindAsync(Id);
        if (profile is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Name) || !SupportedCultures.IsSupported(Language) || !NoteNameSystems.IsSupported(NoteNameSystem))
        {
            return Page();
        }

        profile.Name = Name.Trim();

        var preferences = await db.ProfilePreferences.FindAsync(Id);
        if (preferences is null)
        {
            preferences = new ProfilePreferences { ProfileId = Id, Language = Language, NoteNameSystem = NoteNameSystem, SessionLengthMinutes = SessionLengthMinutes };
            db.ProfilePreferences.Add(preferences);
        }
        else
        {
            preferences.Language = Language;
            preferences.NoteNameSystem = NoteNameSystem;
            preferences.SessionLengthMinutes = SessionLengthMinutes;
        }

        await db.SaveChangesAsync();

        var currentProfileId = await profiles.GetOrCreateProfileIdAsync(HttpContext);
        if (currentProfileId == Id)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(Language)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        }

        return RedirectToPage("/Profiles/Index");
    }
}
