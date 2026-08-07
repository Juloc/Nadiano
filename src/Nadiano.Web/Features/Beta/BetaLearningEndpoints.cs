using System.Text;

using Nadiano.Core.Beta;
using Nadiano.Core.Content;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Beta;

public static class BetaLearningEndpoints
{
    public static IEndpointRouteBuilder MapBetaLearningEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/beta/cards/{kind}/{seed:int}", GetCard);
        endpoints.MapGet("/api/beta/curriculum", GetCurriculum);
        endpoints.MapGet("/api/beta/release-content", GetReleaseContent);
        endpoints.MapGet("/api/beta/repertoire/{id}/score", GetRepertoireScore);
        endpoints.MapGet("/api/beta/repertoire/{id}/expected-events", GetRepertoireExpectedEvents);
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

    private static IResult GetReleaseContent() => Results.Ok(ReleaseContentCatalogue.Create());

    private static IResult GetRepertoireScore(string id)
    {
        var piece = ReleaseContentCatalogue.FindRepertoire(id);
        return piece is null
            ? Results.NotFound()
            : Results.Text(
                ReleaseContentCatalogue.CreateMusicXml(piece),
                "application/vnd.recordare.musicxml+xml",
                Encoding.UTF8);
    }

    private static IResult GetRepertoireExpectedEvents(string id)
    {
        var piece = ReleaseContentCatalogue.FindRepertoire(id);
        if (piece is null)
        {
            return Results.NotFound();
        }

        var generated = MusicXmlExpectedEventGenerator.Generate(
            ReleaseContentCatalogue.CreateMusicXml(piece),
            defaultTempoBpm: piece.TargetTempoBpm);
        return generated.Document is null
            ? Results.UnprocessableEntity(new { warnings = generated.UnsupportedConstructs })
            : Results.Ok(generated.Document);
    }

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
