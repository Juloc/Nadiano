using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Nadiano.Web.Features.Library;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Profiles;

public class DeleteModel(
    NadianoDbContext db,
    CurrentProfileAccessor profiles,
    PrivateLibraryStorage libraryStorage) : PageModel
{
    [BindProperty]
    public Guid Id { get; set; }

    public string ProfileName { get; private set; } = string.Empty;

    public int SessionCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var profile = await db.LearnerProfiles.FindAsync(id);
        if (profile is null)
        {
            return NotFound();
        }

        Id = profile.Id;
        ProfileName = profile.Name;
        SessionCount = await db.PracticeSessions.CountAsync(item => item.ProfileId == id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var profile = await db.LearnerProfiles.FindAsync(Id);
        if (profile is null)
        {
            return NotFound();
        }

        var currentProfileId = await profiles.GetOrCreateProfileIdAsync(HttpContext);
        var profileDirectory = Path.Combine(libraryStorage.RootPath, Id.ToString("N"));
        var quarantineDirectory = Path.Combine(libraryStorage.StagingPath, $"deleted-profile-{Id:N}-{Guid.NewGuid():N}");
        var movedLibrary = false;

        if (Directory.Exists(profileDirectory))
        {
            Directory.Move(profileDirectory, quarantineDirectory);
            movedLibrary = true;
        }

        try
        {
            db.LearnerProfiles.Remove(profile);
            await db.SaveChangesAsync();
        }
        catch
        {
            if (movedLibrary && Directory.Exists(quarantineDirectory) && !Directory.Exists(profileDirectory))
            {
                Directory.Move(quarantineDirectory, profileDirectory);
            }
            throw;
        }

        if (movedLibrary && Directory.Exists(quarantineDirectory))
        {
            Directory.Delete(quarantineDirectory, recursive: true);
        }

        if (currentProfileId == Id)
        {
            Response.Cookies.Delete(CurrentProfileAccessor.CookieName);
        }

        return RedirectToPage("/Profiles/Index");
    }
}
