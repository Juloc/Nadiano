using System.IO.Compression;
using System.Text.Json;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Manifests;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.Features.Library;

public static class PrivateLibraryEndpoints
{
    public static IEndpointRouteBuilder MapPrivateLibraryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/library/{itemId:guid}/score", ServeScoreAsync);
        endpoints.MapGet("/api/library/{itemId:guid}/expected-events", ServeExpectedEventsAsync);
        endpoints.MapGet("/api/library/{itemId:guid}/export", ExportOriginalAsync);
        endpoints.MapGet("/api/library/{itemId:guid}/package", ExportPackageAsync);
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
        if (result is null)
        {
            return Results.NotFound();
        }

        context.Response.Headers.CacheControl = "private, no-store";
        return Results.File(result.Value.Path, "application/vnd.recordare.musicxml+xml", enableRangeProcessing: false);
    }

    private static async Task<IResult> ServeExpectedEventsAsync(
        Guid itemId,
        HttpContext context,
        CurrentProfileAccessor profiles,
        PrivateLibraryService library,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(context, cancellationToken);
        var result = await library.ResolveFileAsync(profileId, itemId, original: false, cancellationToken);
        if (result is null)
        {
            return Results.NotFound();
        }

        var metadata = library.DeserializeMetadata(result.Value.Item.MetadataJson);
        var partMapping = BuildPartMapping(metadata);
        var musicXml = await File.ReadAllTextAsync(result.Value.Path, cancellationToken);
        var generated = MusicXmlExpectedEventGenerator.Generate(musicXml, partMapping, metadata.TargetTempoBpm);
        context.Response.Headers.CacheControl = "private, no-store";
        if (generated.Document is null)
        {
            return Results.UnprocessableEntity(new { warnings = generated.UnsupportedConstructs });
        }

        return Results.Ok(ApplyOverrides(generated.Document, metadata));
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
        if (result is null)
        {
            return Results.NotFound();
        }

        context.Response.Headers.CacheControl = "private, no-store";
        return Results.File(
            result.Value.Path,
            "application/octet-stream",
            result.Value.Item.SourceFileName,
            enableRangeProcessing: false);
    }

    private static async Task<IResult> ExportPackageAsync(
        Guid itemId,
        HttpContext context,
        CurrentProfileAccessor profiles,
        PrivateLibraryService library,
        CancellationToken cancellationToken)
    {
        var profileId = await profiles.GetOrCreateProfileIdAsync(context, cancellationToken);
        var original = await library.ResolveFileAsync(profileId, itemId, original: true, cancellationToken);
        var normalized = await library.ResolveFileAsync(profileId, itemId, original: false, cancellationToken);
        if (original is null || normalized is null)
        {
            return Results.NotFound();
        }

        var item = original.Value.Item;
        var manifest = new
        {
            schemaVersion = 1,
            item.Id,
            item.DisplayTitle,
            item.SourceFileName,
            item.OriginalSha256,
            item.ContentLength,
            item.ValidationState,
            warnings = JsonSerializer.Deserialize<string[]>(item.WarningJson) ?? [],
            metadata = library.DeserializeMetadata(item.MetadataJson),
            item.Version,
            item.ImportedAtUtc,
        };

        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            await CopyIntoArchiveAsync(
                archive,
                $"original/{Path.GetFileName(item.SourceFileName)}",
                original.Value.Path,
                cancellationToken);
            await CopyIntoArchiveAsync(archive, "score.musicxml", normalized.Value.Path, cancellationToken);
            var manifestEntry = archive.CreateEntry("nadiano-package.json", CompressionLevel.Optimal);
            await using var manifestStream = manifestEntry.Open();
            await JsonSerializer.SerializeAsync(manifestStream, manifest, cancellationToken: cancellationToken);
        }

        context.Response.Headers.CacheControl = "private, no-store";
        var safeName = string.Concat(item.DisplayTitle.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '-' : character)).Trim();
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "nadiano-score";
        }
        return Results.File(output.ToArray(), "application/zip", $"{safeName}.nadiano.zip");
    }

    private static async Task CopyIntoArchiveAsync(
        ZipArchive archive,
        string entryName,
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var target = entry.Open();
        await using var source = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await source.CopyToAsync(target, cancellationToken);
    }

    private static IReadOnlyDictionary<string, Hand> BuildPartMapping(LibraryItemMetadata metadata)
    {
        var mapping = new Dictionary<string, Hand>(StringComparer.Ordinal);
        if (metadata.LeftHandPartId is not null && metadata.LeftHandPartId != metadata.RightHandPartId)
        {
            mapping[metadata.LeftHandPartId] = Hand.Left;
        }
        if (metadata.RightHandPartId is not null && metadata.RightHandPartId != metadata.LeftHandPartId)
        {
            mapping[metadata.RightHandPartId] = Hand.Right;
        }
        return mapping;
    }

    private static ExpectedEventDocument ApplyOverrides(ExpectedEventDocument source, LibraryItemMetadata metadata)
    {
        var events = source.Events.Select(item =>
        {
            var hand = item.Hand;
            if (item.Voice is not null && item.Voice == metadata.LeftHandVoice)
            {
                hand = Hand.Left;
            }
            if (item.Voice is not null && item.Voice == metadata.RightHandVoice)
            {
                hand = Hand.Right;
            }

            var fingering = metadata.FingeringOverrides.TryGetValue(item.Id, out var configured)
                ? configured
                : item.Fingering;
            return item with { Hand = hand, Fingering = fingering };
        }).ToArray();

        return new ExpectedEventDocument
        {
            SchemaVersion = source.SchemaVersion,
            TimeBase = source.TimeBase,
            TempoMap = source.TempoMap,
            Events = events,
        };
    }
}