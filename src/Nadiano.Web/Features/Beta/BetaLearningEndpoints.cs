using Nadiano.Core.Beta;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Beta;

public static class BetaLearningEndpoints
{
    public static IEndpointRouteBuilder MapBetaLearningEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/beta/cards/{kind}/{seed:int}", GetCard);
        endpoints.MapGet("/api/beta/curriculum", GetCurriculum);
        endpoints.MapGet("/api/beta/reviews", GetReviewsAsync);
        endpoints.MapGet("/api/beta/session-plan", GetSessionPlanAsync);
        endpoints.MapPost("/api/beta/evidence", RecordEvidenceAsync);
        return endpoints;
    }

    private static IResult GetCard(string kind, int seed, BetaLearningService service)
    {
        var exerciseKind = kind.Equals("rhythm", StringComparison.OrdinalIgnoreCase)
            ? GeneratedExerciseKind.Rhythm
            : GeneratedExerciseKind.Reading;
        return Results.Ok(service.GenerateCard(exerciseKind, seed));
    }

    private static IResult GetCurriculum(BetaLearningService service) => Results.Ok(service.Curriculum);

    private static async Task<IResult> GetReviewsAsync(
        HttpContext context,
        CurrentProfileAccessor profiles,
        BetaLearningService service,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(context, cancellationToken);
        return Results.Ok(await service.DueAsync(profileId, DateTimeOffset.UtcNow, cancellationToken));
    }

    private static async Task<IResult> GetSessionPlanAsync(
        HttpContext context,
        CurrentProfileAccessor profiles,
        BetaLearningService service,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(context, cancellationToken);
        return Results.Ok(await service.BuildSessionPlanAsync(profileId, DateOnly.FromDateTime(DateTime.Today), cancellationToken));
    }

    private static async Task<IResult> RecordEvidenceAsync(
        RecordBetaEvidenceRequest request,
        HttpContext context,
        CurrentProfileAccessor profiles,
        BetaLearningService service,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(context, cancellationToken);
        try
        {
            await service.RecordAsync(profileId, request, cancellationToken);
            return Results.NoContent();
        }
        catch (ArgumentException)
        {
            return Results.BadRequest();
        }
    }
}
