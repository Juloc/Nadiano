using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Library;

public static class PrivateLibraryEndpoints
{
    public static IEndpointRouteBuilder MapPrivateLibraryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/library/{itemId:guid}/score", ServeScoreAsync);
        endpoints.MapGet("/api/library/{itemId:guid}/export", ExportOriginalAsync);
        return endpoints;
    }

    private static async Task<IResult> ServeScoreAsync(
        Guid itemId,
        HttpContext context,
        CurrentProfileAccessor profiles,
        PrivateLibraryService library,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(context, cancellationToken);
        var result = await library.ResolveFileAsync(profileId, itemId, original: false, cancellationToken);
        return result is null
            ? Results.NotFound()
            : Results.File(result.Value.Path, "application/vnd.recordare.musicxml+xml", enableRangeProcessing: false);
    }

    private static async Task<IResult> ExportOriginalAsync(
        Guid itemId,
        HttpContext context,
        CurrentProfileAccessor profiles,
        PrivateLibraryService library,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(context, cancellationToken);
        var result = await library.ResolveFileAsync(profileId, itemId, original: true, cancellationToken);
        return result is null
            ? Results.NotFound()
            : Results.File(
                result.Value.Path,
                "application/octet-stream",
                result.Value.Item.SourceFileName,
                enableRangeProcessing: false);
    }
}
