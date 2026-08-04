using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Pages.Profiles;

public class DeleteModel(NadianoDbContext db, CurrentProfileAccessor profiles) : PageModel
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
        SessionCount = await db.PracticeSessions.CountAsync(s => s.ProfileId == id);

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

        db.LearnerProfiles.Remove(profile);
        await db.SaveChangesAsync();

        if (currentProfileId == Id)
        {
            Response.Cookies.Delete(CurrentProfileAccessor.CookieName);
        }

        return RedirectToPage("/Profiles/Index");
    }
}
