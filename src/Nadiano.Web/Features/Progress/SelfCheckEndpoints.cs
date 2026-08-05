using Nadiano.Core.Progress;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Progress;

/// <summary>
/// Records a learner's self-check answer as evidence (docs/JUNIOR_IMPLEMENTATION_PLAN.md
/// WP-020 step 7). This is never scored or used to unlock content — it is
/// stored for the learner's own record, matching docs/CONTENT_MODEL.md §6:
/// a self-assessment skill must never be treated as an objective pass.
/// </summary>
public static class SelfCheckEndpoints
{
    public static void MapSelfCheckEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/progress/self-checks", RecordAsync);
    }

    private static async Task<IResult> RecordAsync(
        RecordSelfCheckRequest request,
        HttpContext httpContext,
        NadianoDbContext db,
        CurrentProfileAccessor profiles)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(httpContext);

        var evidence = new SkillEvidenceRecord
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            LessonId = request.LessonId,
            SkillId = request.SkillId,
            SelfReportedSuccess = request.SelfReportedSuccess,
            RecordedAtUtc = DateTimeOffset.UtcNow,
        };

        db.SkillEvidence.Add(evidence);
        await db.SaveChangesAsync();

        return Results.Ok(new RecordSelfCheckResponse(evidence.Id));
    }
}

public sealed record RecordSelfCheckRequest(string LessonId, string SkillId, bool SelfReportedSuccess);

public sealed record RecordSelfCheckResponse(Guid EvidenceId);
