using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Profiles;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.Infrastructure.Profiles;

/// <summary>
/// Resolves the current household learner from a cookie, auto-creating an
/// anonymous profile on first visit. The Profiles feature (WP-018) lets the
/// learner rename that default and create/select/delete siblings sharing
/// the same browser.
/// </summary>
public class CurrentProfileAccessor(NadianoDbContext db)
{
    public const string CookieName = "nadiano-profile-id";
    private const string DefaultLanguage = "de";
    private const int DefaultSessionLengthMinutes = 20;

    public async Task<Guid> GetOrCreateProfileIdAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        if (httpContext.Request.Cookies.TryGetValue(CookieName, out var raw) && Guid.TryParse(raw, out var existingId))
        {
            var exists = await db.LearnerProfiles.AnyAsync(p => p.Id == existingId, cancellationToken);
            if (exists)
            {
                return existingId;
            }
        }

        var profile = new LearnerProfile
        {
            Id = Guid.NewGuid(),
            Name = "Profil 1",
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
        db.LearnerProfiles.Add(profile);
        db.ProfilePreferences.Add(new ProfilePreferences
        {
            ProfileId = profile.Id,
            Language = DefaultLanguage,
            NoteNameSystem = NoteNameSystems.German,
            SessionLengthMinutes = DefaultSessionLengthMinutes,
        });
        await db.SaveChangesAsync(cancellationToken);

        SetCurrentProfileCookie(httpContext, profile.Id);

        return profile.Id;
    }

    /// <summary>Switches the browser's current profile to an existing one (the "select" action).</summary>
    public void SetCurrentProfileCookie(HttpContext httpContext, Guid profileId)
    {
        httpContext.Response.Cookies.Append(
            CookieName,
            profileId.ToString(),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(5),
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
            });
    }
}
