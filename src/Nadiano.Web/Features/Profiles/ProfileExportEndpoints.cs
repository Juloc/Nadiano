using Microsoft.EntityFrameworkCore;

using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Profiles;

/// <summary>
/// Complete JSON export of the active profile. The endpoint is restricted to
/// the profile selected by the same-origin profile cookie.
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

        var profile = await db.LearnerProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == profileId);
        if (profile is null)
        {
            return Results.NotFound();
        }

        var preferences = await db.ProfilePreferences.AsNoTracking().SingleOrDefaultAsync(item => item.ProfileId == profileId);
        var sessionRecords = await db.PracticeSessions
            .AsNoTracking()
            .Include(item => item.Attempt)
            .Where(item => item.ProfileId == profileId)
            .ToListAsync();
        var evidenceRecords = await db.LearningEvidence
            .AsNoTracking()
            .Where(item => item.ProfileId == profileId)
            .ToListAsync();
        var reviewRecords = await db.ReviewQueue
            .AsNoTracking()
            .Where(item => item.ProfileId == profileId)
            .ToListAsync();
        var libraryRecords = await db.PrivateLibraryItems
            .AsNoTracking()
            .Where(item => item.ProfileId == profileId)
            .ToListAsync();

        var sessions = sessionRecords
            .OrderBy(item => item.StartedAtUtc)
            .Select(item => new ProfileExportSession(
                item.Id,
                item.LessonId,
                item.ContentVersion,
                item.Mode,
                item.StartedAtUtc,
                item.Attempt is null ? null : new ProfileExportAttempt(
                    item.Attempt.CompletedAtUtc,
                    item.Attempt.ResultSchemaVersion,
                    item.Attempt.ResultJson,
                    item.Attempt.NextActionCode)))
            .ToList();
        var learningEvidence = evidenceRecords
            .OrderBy(item => item.RecordedAtUtc)
            .Select(item => new ProfileExportEvidence(
                item.ActivityId,
                item.ActivityKind,
                item.Seed,
                item.ExpectedJson,
                item.ResponseJson,
                item.ResultJson,
                item.RecordedAtUtc))
            .ToList();
        var reviewQueue = reviewRecords
            .OrderBy(item => item.DueAtUtc)
            .Select(item => new ProfileExportReview(
                item.SkillId,
                item.SourceId,
                item.DueAtUtc,
                item.IntervalDays,
                item.ReasonCode,
                item.UpdatedAtUtc))
            .ToList();
        var library = libraryRecords
            .OrderBy(item => item.ImportedAtUtc)
            .Select(item => new ProfileExportLibraryItem(
                item.Id,
                item.DisplayTitle,
                item.SourceFileName,
                item.OriginalSha256,
                item.ContentLength,
                item.ValidationState,
                item.WarningJson,
                item.MetadataJson,
                item.Version,
                item.ImportedAtUtc))
            .ToList();

        var export = new ProfileExport(
            profile.Id,
            profile.Name,
            profile.CreatedAtUtc,
            preferences is null ? null : new ProfileExportPreferences(preferences.Language, preferences.NoteNameSystem, preferences.SessionLengthMinutes),
            sessions,
            learningEvidence,
            reviewQueue,
            library);

        var fileName = $"nadiano-profil-{profile.Id}.json";
        var json = System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true,
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
    List<ProfileExportSession> Sessions,
    List<ProfileExportEvidence> LearningEvidence,
    List<ProfileExportReview> ReviewQueue,
    List<ProfileExportLibraryItem> PrivateLibrary);

public sealed record ProfileExportPreferences(string Language, string NoteNameSystem, int SessionLengthMinutes);

public sealed record ProfileExportSession(
    Guid Id,
    string LessonId,
    string ContentVersion,
    string Mode,
    DateTimeOffset StartedAtUtc,
    ProfileExportAttempt? Attempt);

public sealed record ProfileExportAttempt(DateTimeOffset CompletedAtUtc, int ResultSchemaVersion, string ResultJson, string NextActionCode);

public sealed record ProfileExportEvidence(
    string ActivityId,
    string ActivityKind,
    int? Seed,
    string ExpectedJson,
    string ResponseJson,
    string ResultJson,
    DateTimeOffset RecordedAtUtc);

public sealed record ProfileExportReview(
    string SkillId,
    string SourceId,
    DateTimeOffset DueAtUtc,
    int IntervalDays,
    string ReasonCode,
    DateTimeOffset UpdatedAtUtc);

public sealed record ProfileExportLibraryItem(
    Guid Id,
    string DisplayTitle,
    string SourceFileName,
    string OriginalSha256,
    long ContentLength,
    string ValidationState,
    string WarningJson,
    string MetadataJson,
    int Version,
    DateTimeOffset ImportedAtUtc);