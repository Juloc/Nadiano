using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Beta;
using Nadiano.Core.Content;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.Features.Library;

public sealed record LibraryImportResult(bool Success, Guid? ItemId, string? ErrorCode, IReadOnlyList<string> Warnings);

public sealed record LibraryItemMetadata(
    int PartCount,
    int MeasureCount,
    int FingeringCount,
    int FromMeasure,
    int ToMeasure,
    int TargetTempoBpm);

public sealed class PrivateLibraryService(
    NadianoDbContext db,
    PrivateLibraryStorage storage,
    ILogger<PrivateLibraryService> logger)
{
    public const long MaximumUploadBytes = 8 * 1024 * 1024;
    private const long MaximumExpandedBytes = 16 * 1024 * 1024;
    private const int MaximumArchiveEntries = 64;
    private const int MaximumXmlCharacters = 4 * 1024 * 1024;

    public async Task<IReadOnlyList<PrivateLibraryItem>> ListAsync(Guid profileId, CancellationToken cancellationToken)
    {
        var items = await db.PrivateLibraryItems
            .Where(item => item.ProfileId == profileId)
            .ToListAsync(cancellationToken);

        return items.OrderByDescending(item => item.ImportedAtUtc).ToArray();
    }

    public async Task<LibraryImportResult> ImportAsync(
        Guid profileId,
        IFormFile upload,
        string? requestedTitle,
        CancellationToken cancellationToken)
    {
        if (upload.Length is <= 0 or > MaximumUploadBytes)
        {
            return new(false, null, "invalid-size", []);
        }

        var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
        if (extension is not ".xml" and not ".musicxml" and not ".mxl")
        {
            return new(false, null, "invalid-type", []);
        }

        var itemId = Guid.NewGuid();
        var stagingDirectory = Path.Combine(storage.StagingPath, itemId.ToString("N"));
        Directory.CreateDirectory(stagingDirectory);

        try
        {
            var originalPath = Path.Combine(stagingDirectory, $"original{extension}");
            await using (var target = new FileStream(originalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
            {
                await upload.CopyToAsync(target, cancellationToken);
            }

            var sha256 = await ComputeSha256Async(originalPath, cancellationToken);
            var musicXml = extension == ".mxl"
                ? await ReadMxlAsync(originalPath, stagingDirectory, cancellationToken)
                : await ReadLimitedTextAsync(originalPath, MaximumXmlCharacters, cancellationToken);

            var document = LoadSecureDocument(musicXml);
            if (document.Root?.Name.LocalName != "score-partwise")
            {
                return new(false, null, "not-score-partwise", []);
            }

            var warnings = new List<string>();
            var generated = MusicXmlExpectedEventGenerator.Generate(musicXml);
            warnings.AddRange(generated.UnsupportedConstructs.Distinct(StringComparer.Ordinal));

            var metadata = ReadMetadata(document);
            var displayTitle = CleanTitle(requestedTitle)
                ?? CleanTitle(ReadTitle(document))
                ?? Path.GetFileNameWithoutExtension(upload.FileName);

            var storedDirectoryName = itemId.ToString("N");
            var finalDirectory = storage.ItemDirectory(profileId, storedDirectoryName);
            Directory.CreateDirectory(Path.GetDirectoryName(finalDirectory)!);
            Directory.Move(stagingDirectory, finalDirectory);

            await File.WriteAllTextAsync(Path.Combine(finalDirectory, "score.musicxml"), musicXml, cancellationToken);

            var item = new PrivateLibraryItem
            {
                Id = itemId,
                ProfileId = profileId,
                DisplayTitle = displayTitle,
                SourceFileName = Path.GetFileName(upload.FileName),
                StoredDirectoryName = storedDirectoryName,
                OriginalSha256 = sha256,
                ContentLength = upload.Length,
                ValidationState = generated.Document is null ? "warning" : "ready",
                WarningJson = JsonSerializer.Serialize(warnings),
                MetadataJson = JsonSerializer.Serialize(metadata),
                Version = 1,
                ImportedAtUtc = DateTimeOffset.UtcNow,
            };

            db.PrivateLibraryItems.Add(item);
            await db.SaveChangesAsync(cancellationToken);

            return new(true, itemId, null, warnings);
        }
        catch (InvalidDataException ex)
        {
            logger.LogInformation(ex, "Rejected private score import for profile {ProfileId}.", profileId);
            return new(false, null, "invalid-package", []);
        }
        catch (XmlException ex)
        {
            logger.LogInformation(ex, "Rejected malformed MusicXML import for profile {ProfileId}.", profileId);
            return new(false, null, "invalid-xml", []);
        }
        finally
        {
            if (Directory.Exists(stagingDirectory))
            {
                Directory.Delete(stagingDirectory, recursive: true);
            }
        }
    }

    public async Task<bool> UpdateAsync(
        Guid profileId,
        Guid itemId,
        string? title,
        int fromMeasure,
        int toMeasure,
        int targetTempoBpm,
        CancellationToken cancellationToken)
    {
        var item = await db.PrivateLibraryItems
            .SingleOrDefaultAsync(candidate => candidate.ProfileId == profileId && candidate.Id == itemId, cancellationToken);
        if (item is null)
        {
            return false;
        }

        var existing = JsonSerializer.Deserialize<LibraryItemMetadata>(item.MetadataJson)
            ?? new LibraryItemMetadata(1, 1, 0, 1, 1, 90);
        var maximumMeasure = Math.Max(1, existing.MeasureCount);
        var safeFrom = Math.Clamp(fromMeasure, 1, maximumMeasure);
        var safeTo = Math.Clamp(toMeasure, safeFrom, maximumMeasure);
        var safeTempo = Math.Clamp(targetTempoBpm, 30, 240);

        item.DisplayTitle = CleanTitle(title) ?? item.DisplayTitle;
        item.MetadataJson = JsonSerializer.Serialize(existing with
        {
            FromMeasure = safeFrom,
            ToMeasure = safeTo,
            TargetTempoBpm = safeTempo,
        });
        item.Version++;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid profileId, Guid itemId, CancellationToken cancellationToken)
    {
        var item = await db.PrivateLibraryItems
            .SingleOrDefaultAsync(candidate => candidate.ProfileId == profileId && candidate.Id == itemId, cancellationToken);
        if (item is null)
        {
            return false;
        }

        db.PrivateLibraryItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);

        var directory = storage.ItemDirectory(profileId, item.StoredDirectoryName);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return true;
    }

    public async Task<(PrivateLibraryItem Item, string Path)?> ResolveFileAsync(
        Guid profileId,
        Guid itemId,
        bool original,
        CancellationToken cancellationToken)
    {
        var item = await db.PrivateLibraryItems
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.ProfileId == profileId && candidate.Id == itemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var directory = storage.ItemDirectory(profileId, item.StoredDirectoryName);
        var path = original
            ? Directory.EnumerateFiles(directory, "original.*").SingleOrDefault()
            : Path.Combine(directory, "score.musicxml");

        return path is not null && File.Exists(path) ? (item, path) : null;
    }

    public void RemoveAbandonedStagingData(TimeSpan maximumAge)
    {
        var cutoff = DateTime.UtcNow.Subtract(maximumAge);
        foreach (var directory in Directory.EnumerateDirectories(storage.StagingPath))
        {
            if (Directory.GetCreationTimeUtc(directory) < cutoff)
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    private static async Task<string> ReadMxlAsync(string path, string stagingDirectory, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        if (archive.Entries.Count > MaximumArchiveEntries)
        {
            throw new InvalidDataException("Archive contains too many entries.");
        }

        long expandedBytes = 0;
        foreach (var entry in archive.Entries)
        {
            expandedBytes += entry.Length;
            if (expandedBytes > MaximumExpandedBytes)
            {
                throw new InvalidDataException("Archive expands beyond the allowed size.");
            }

            if (entry.FullName.Contains('\0', StringComparison.Ordinal) ||
                entry.FullName.StartsWith('/', StringComparison.Ordinal) ||
                entry.FullName.Contains("..", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Archive contains an unsafe path.");
            }

            var fullPath = Path.GetFullPath(Path.Combine(stagingDirectory, entry.FullName));
            if (!fullPath.StartsWith(Path.GetFullPath(stagingDirectory) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Archive path escaped the staging directory.");
            }
        }

        var scoreEntry = FindScoreEntry(archive);
        if (scoreEntry is null || scoreEntry.Length > MaximumXmlCharacters)
        {
            throw new InvalidDataException("Archive does not contain a supported score document.");
        }

        await using var scoreStream = scoreEntry.Open();
        using var reader = new StreamReader(scoreStream);
        var score = await reader.ReadToEndAsync(cancellationToken);
        if (score.Length > MaximumXmlCharacters)
        {
            throw new InvalidDataException("MusicXML document is too large.");
        }

        return score;
    }

    private static ZipArchiveEntry? FindScoreEntry(ZipArchive archive)
    {
        var container = archive.GetEntry("META-INF/container.xml");
        if (container is not null && container.Length <= 64 * 1024)
        {
            using var stream = container.Open();
            using var reader = XmlReader.Create(stream, SecureSettings(64 * 1024));
            var document = XDocument.Load(reader);
            var rootFile = document.Descendants().FirstOrDefault(element => element.Name.LocalName == "rootfile")
                ?.Attribute("full-path")?.Value;
            if (!string.IsNullOrWhiteSpace(rootFile))
            {
                return archive.GetEntry(rootFile);
            }
        }

        return archive.Entries.FirstOrDefault(entry =>
            Path.GetExtension(entry.FullName).Equals(".musicxml", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(entry.FullName).Equals(".xml", StringComparison.OrdinalIgnoreCase));
    }

    private static XDocument LoadSecureDocument(string xml)
    {
        using var textReader = new StringReader(xml);
        using var reader = XmlReader.Create(textReader, SecureSettings(MaximumXmlCharacters));
        return XDocument.Load(reader, LoadOptions.None);
    }

    private static XmlReaderSettings SecureSettings(long maximumCharacters) => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        MaxCharactersInDocument = maximumCharacters,
        MaxCharactersFromEntities = 0,
    };

    private static LibraryItemMetadata ReadMetadata(XDocument document)
    {
        var root = document.Root!;
        var parts = root.Elements().Count(element => element.Name.LocalName == "part");
        var measures = root.Descendants().Count(element => element.Name.LocalName == "measure");
        var fingerings = root.Descendants().Count(element => element.Name.LocalName == "fingering");
        var maximumMeasure = Math.Max(1, measures / Math.Max(1, parts));
        return new LibraryItemMetadata(parts, maximumMeasure, fingerings, 1, maximumMeasure, 90);
    }

    private static string? ReadTitle(XDocument document) =>
        document.Descendants().FirstOrDefault(element => element.Name.LocalName == "work-title")?.Value
        ?? document.Descendants().FirstOrDefault(element => element.Name.LocalName == "movement-title")?.Value;

    private static string? CleanTitle(string? title)
    {
        var value = title?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value[..Math.Min(value.Length, 300)];
    }

    private static async Task<string> ReadLimitedTextAsync(string path, int maximumCharacters, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(path);
        var buffer = new char[maximumCharacters + 1];
        var read = await reader.ReadBlockAsync(buffer.AsMemory(), cancellationToken);
        if (read > maximumCharacters || !reader.EndOfStream)
        {
            throw new InvalidDataException("MusicXML document is too large.");
        }

        return new string(buffer, 0, read);
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
