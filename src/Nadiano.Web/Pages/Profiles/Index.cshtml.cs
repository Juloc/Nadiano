using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Profiles;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Profiles;

public class IndexModel(NadianoDbContext db, CurrentProfileAccessor profiles) : PageModel
{
    public IReadOnlyList<LearnerProfile> Profiles { get; private set; } = [];

    public Guid CurrentProfileId { get; private set; }

    [BindProperty]
    public string NewProfileName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        CurrentProfileId = await profiles.GetOrCreateProfileIdAsync(HttpContext);
        Profiles = (await db.LearnerProfiles.ToListAsync()).OrderBy(p => p.CreatedAtUtc).ToList();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var name = string.IsNullOrWhiteSpace(NewProfileName) ? $"Profil {await db.LearnerProfiles.CountAsync() + 1}" : NewProfileName.Trim();

        var profile = new LearnerProfile { Id = Guid.NewGuid(), Name = name, CreatedAtUtc = DateTimeOffset.UtcNow };
        db.LearnerProfiles.Add(profile);
        db.ProfilePreferences.Add(new ProfilePreferences
        {
            ProfileId = profile.Id,
            Language = "de",
            NoteNameSystem = NoteNameSystems.German,
            SessionLengthMinutes = 20,
        });
        await db.SaveChangesAsync();

        profiles.SetCurrentProfileCookie(HttpContext, profile.Id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSelectAsync(Guid id)
    {
        var exists = await db.LearnerProfiles.AnyAsync(p => p.Id == id);
        if (exists)
        {
            profiles.SetCurrentProfileCookie(HttpContext, id);
        }

        return RedirectToPage("/Index");
    }
}
