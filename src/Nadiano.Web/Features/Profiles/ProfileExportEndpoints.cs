using Microsoft.EntityFrameworkCore;

using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Profiles;

/// <summary>
/// JSON export of one profile's data (docs/JUNIOR_IMPLEMENTATION_PLAN.md
/// WP-018). Scoped to the requesting cookie's own profile — same
/// same-origin-cookie reasoning as PracticeSessionEndpoints.
/// </summary>
public static class ProfileExportEndpoints
{
    public static void MapProfileExportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/profiles/{profileId:guid}/export", ExportAsync);
    }

    private static async Task<IResult> ExportAsync(
        Guid profileId,
        HttpContext httpContext,
        NadianoDbContext db,
        CurrentProfileAccessor profiles)
    {
        var currentProfileId = await profiles.GetOrCreateProfileIdAsync(httpContext);
        if (profileId != currentProfileId)
        {
            return Results.NotFound();
        }

        var profile = await db.LearnerProfiles.FindAsync(profileId);
        if (profile is null)
        {
            return Results.NotFound();
        }

        var preferences = await db.ProfilePreferences.FindAsync(profileId);
        var sessions = (await db.PracticeSessions
            .Include(s => s.Attempt)
            .Where(s => s.ProfileId == profileId)
            .ToListAsync())
            .OrderBy(s => s.StartedAtUtc)
            .ToList();

        var export = new ProfileExport(
            profile.Id,
            profile.Name,
            profile.CreatedAtUtc,
            preferences is null ? null : new ProfileExportPreferences(preferences.Language, preferences.NoteNameSystem, preferences.SessionLengthMinutes),
            sessions.Select(s => new ProfileExportSession(
                s.Id,
                s.LessonId,
                s.ContentVersion,
                s.Mode,
                s.StartedAtUtc,
                s.Attempt is null ? null : new ProfileExportAttempt(s.Attempt.CompletedAtUtc, s.Attempt.ResultSchemaVersion, s.Attempt.ResultJson, s.Attempt.NextActionCode)))
                .ToList());

        var fileName = $"nadiano-profil-{profile.Id}.json";
        var json = System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        });
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(json),
            contentType: "application/json",
            fileDownloadName: fileName);
    }
}

public sealed record ProfileExport(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAtUtc,
    ProfileExportPreferences? Preferences,
    List<ProfileExportSession> Sessions);

public sealed record ProfileExportPreferences(string Language, string NoteNameSystem, int SessionLengthMinutes);

public sealed record ProfileExportSession(
    Guid Id,
    string LessonId,
    string ContentVersion,
    string Mode,
    DateTimeOffset StartedAtUtc,
    ProfileExportAttempt? Attempt);

public sealed record ProfileExportAttempt(DateTimeOffset CompletedAtUtc, int ResultSchemaVersion, string ResultJson, string NextActionCode);
