using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Practice;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Practice;

/// <summary>
/// JSON endpoints the practice workspace calls to persist an attempt
/// (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-017). Session/attempt ids are
/// client-generated so completion is idempotent under retry.
///
/// These endpoints rely on the profile-id cookie (SameSite=Strict,
/// HttpOnly) as the same-origin mechanism required by
/// docs/TECHNICAL_ARCHITECTURE.md §10 rather than a classic antiforgery
/// token: a cross-site request cannot carry the cookie, so it would only
/// ever create/complete a session under a fresh anonymous profile it has
/// no way to read back — never another household member's data.
/// </summary>
public static class PracticeSessionEndpoints
{
    public static void MapPracticeSessionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/practice/sessions", CreateSessionAsync);
        app.MapPost("/api/practice/sessions/{sessionId:guid}/complete", CompleteSessionAsync);
        app.MapGet("/api/practice/sessions/{sessionId:guid}", GetSessionAsync);
    }

    private static async Task<IResult> CreateSessionAsync(
        CreateSessionRequest request,
        HttpContext httpContext,
        NadianoDbContext db,
        CurrentProfileAccessor profiles,
        CourseProgressService progress)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(httpContext);

        var existing = await db.PracticeSessions.FindAsync(request.SessionId);
        if (existing is not null)
        {
            return existing.ProfileId == profileId ? Results.Ok(new SessionCreatedResponse(existing.Id)) : Results.NotFound();
        }

        // Manual URL/API navigation to a locked lesson must not be able to start a
        // session for it (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-019 acceptance criteria).
        if (!await progress.IsLessonAvailableAsync(profileId, request.LessonId))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var session = new PracticeSessionRecord
        {
            Id = request.SessionId,
            ProfileId = profileId,
            LessonId = request.LessonId,
            ContentVersion = request.ContentVersion,
            Mode = request.Mode,
            StartedAtUtc = DateTimeOffset.UtcNow,
        };

        db.PracticeSessions.Add(session);
        await db.SaveChangesAsync();

        return Results.Ok(new SessionCreatedResponse(session.Id));
    }

    private static async Task<IResult> CompleteSessionAsync(
        Guid sessionId,
        CompleteSessionRequest request,
        HttpContext httpContext,
        NadianoDbContext db,
        CurrentProfileAccessor profiles,
        CourseProgressService progress)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(httpContext);

        var session = await db.PracticeSessions.Include(s => s.Attempt).FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null || session.ProfileId != profileId)
        {
            return Results.NotFound();
        }

        // Idempotent: a resubmission (e.g. the first response was lost) returns the
        // attempt already stored rather than creating a duplicate or erroring.
        if (session.Attempt is not null)
        {
            return Results.Ok(ToResponse(session.Attempt));
        }

        var attempt = new PracticeAttemptRecord
        {
            Id = request.AttemptId,
            SessionId = session.Id,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            ResultSchemaVersion = request.ResultSchemaVersion,
            ResultJson = request.ResultJson,
            NextActionCode = request.NextActionCode,
        };

        db.PracticeAttempts.Add(attempt);
        await db.SaveChangesAsync();

        await progress.EvaluateAndRecordCompletionAsync(profileId, session.LessonId);

        return Results.Ok(ToResponse(attempt));
    }

    private static async Task<IResult> GetSessionAsync(Guid sessionId, HttpContext httpContext, NadianoDbContext db, CurrentProfileAccessor profiles)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(httpContext);

        var session = await db.PracticeSessions.Include(s => s.Attempt).FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null || session.ProfileId != profileId)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            session.Id,
            session.LessonId,
            session.ContentVersion,
            session.Mode,
            session.StartedAtUtc,
            Attempt = session.Attempt is null ? null : ToResponse(session.Attempt),
        });
    }

    private static AttemptResponse ToResponse(PracticeAttemptRecord attempt) =>
        new(attempt.Id, attempt.CompletedAtUtc, attempt.ResultSchemaVersion, attempt.ResultJson, attempt.NextActionCode);
}