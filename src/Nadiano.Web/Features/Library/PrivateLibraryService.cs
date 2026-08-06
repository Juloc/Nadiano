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

public sealed record LibraryPartMetadata(string Id, string Name, IReadOnlyList<string> Voices);

public sealed record LibraryItemMetadata(
    int PartCount,
    int MeasureCount,
    int FingeringCount,
    int FromMeasure,
    int ToMeasure,
    int TargetTempoBpm)
{
    public IReadOnlyList<LibraryPartMetadata> Parts { get; init; } = [];
    public string? LeftHandPartId { get; init; }
    public string? LeftHandVoice { get; init; }
    public string? RightHandPartId { get; init; }
    public string? RightHandVoice { get; init; }
    public IReadOnlyDictionary<string, int[]> FingeringOverrides { get; init; } = new Dictionary<string, int[]>(StringComparer.Ordinal);
}

public sealed record LibraryEditRequest(
    string? Title,
    int FromMeasure,
    int ToMeasure,
    int TargetTempoBpm,
    string? LeftHandPartId,
    string? LeftHandVoice,
    string? RightHandPartId,
    string? RightHandVoice,
    string? FingeringOverridesText);

public sealed record LibraryUpdateResult(bool Success, string? ErrorCode);

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

    public async Task<LibraryUpdateResult> UpdateAsync(
        Guid profileId,
        Guid itemId,
        LibraryEditRequest update,
        CancellationToken cancellationToken)
    {
        var item = await db.PrivateLibraryItems
            .SingleOrDefaultAsync(candidate => candidate.ProfileId == profileId && candidate.Id == itemId, cancellationToken);
        if (item is null)
        {
            return new(false, "not-found");
        }

        var existing = DeserializeMetadata(item.MetadataJson);
        var leftPart = NormalizePart(existing.Parts, update.LeftHandPartId);
        var rightPart = NormalizePart(existing.Parts, update.RightHandPartId);
        if (HasValue(update.LeftHandPartId) && leftPart is null || HasValue(update.RightHandPartId) && rightPart is null)
        {
            return new(false, "invalid-part");
        }

        var leftVoice = NormalizeVoice(existing.Parts, leftPart, update.LeftHandVoice);
        var rightVoice = NormalizeVoice(existing.Parts, rightPart, update.RightHandVoice);
        if (HasValue(update.LeftHandVoice) && leftVoice is null || HasValue(update.RightHandVoice) && rightVoice is null)
        {
            return new(false, "invalid-voice");
        }

        if (!TryParseFingeringOverrides(update.FingeringOverridesText, out var fingeringOverrides))
        {
            return new(false, "invalid-fingering");
        }

        var maximumMeasure = Math.Max(1, existing.MeasureCount);
        var safeFrom = Math.Clamp(update.FromMeasure, 1, maximumMeasure);
        var safeTo = Math.Clamp(update.ToMeasure, safeFrom, maximumMeasure);
        var safeTempo = Math.Clamp(update.TargetTempoBpm, 30, 240);

        item.DisplayTitle = CleanTitle(update.Title) ?? item.DisplayTitle;
        item.MetadataJson = JsonSerializer.Serialize(existing with
        {
            FromMeasure = safeFrom,
            ToMeasure = safeTo,
            TargetTempoBpm = safeTempo,
            LeftHandPartId = leftPart,
            LeftHandVoice = leftVoice,
            RightHandPartId = rightPart,
            RightHandVoice = rightVoice,
            FingeringOverrides = fingeringOverrides,
        });
        item.Version++;
        await db.SaveChangesAsync(cancellationToken);
        return new(true, null);
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

    public LibraryItemMetadata DeserializeMetadata(string json)
    {
        var metadata = JsonSerializer.Deserialize<LibraryItemMetadata>(json)
            ?? new LibraryItemMetadata(1, 1, 0, 1, 1, 90);
        return metadata with
        {
            Parts = metadata.Parts ?? [],
            FingeringOverrides = metadata.FingeringOverrides ?? new Dictionary<string, int[]>(StringComparer.Ordinal),
        };
    }

    public static string FormatFingeringOverrides(IReadOnlyDictionary<string, int[]> overrides) =>
        string.Join(Environment.NewLine, overrides
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => $"{item.Key}={string.Join(',', item.Value)}"));

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
                entry.FullName.StartsWith("/", StringComparison.Ordinal) ||
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
        var partNames = root.Descendants()
            .Where(element => element.Name.LocalName == "score-part")
            .Where(element => element.Attribute("id") is not null)
            .ToDictionary(
                element => element.Attribute("id")!.Value,
                element => element.Elements().FirstOrDefault(child => child.Name.LocalName == "part-name")?.Value.Trim()
                    ?? element.Attribute("id")!.Value,
                StringComparer.Ordinal);
        var parts = root.Elements()
            .Where(element => element.Name.LocalName == "part")
            .Select(element =>
            {
                var id = element.Attribute("id")?.Value ?? string.Empty;
                var voices = element.Descendants()
                    .Where(child => child.Name.LocalName == "voice")
                    .Select(child => child.Value.Trim())
                    .Where(value => value.Length > 0)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                return new LibraryPartMetadata(id, partNames.GetValueOrDefault(id, id), voices);
            })
            .Where(part => part.Id.Length > 0)
            .ToArray();
        var measures = root.Descendants().Count(element => element.Name.LocalName == "measure");
        var fingerings = root.Descendants().Count(element => element.Name.LocalName == "fingering");
        var maximumMeasure = Math.Max(1, measures / Math.Max(1, parts.Length));
        return new LibraryItemMetadata(parts.Length, maximumMeasure, fingerings, 1, maximumMeasure, 90)
        {
            Parts = parts,
        };
    }

    private static string? NormalizePart(IReadOnlyList<LibraryPartMetadata> parts, string? value)
    {
        var clean = CleanOptional(value);
        return clean is null || parts.Any(part => part.Id == clean) ? clean : null;
    }

    private static string? NormalizeVoice(IReadOnlyList<LibraryPartMetadata> parts, string? partId, string? value)
    {
        var clean = CleanOptional(value);
        if (clean is null)
        {
            return null;
        }

        var part = parts.SingleOrDefault(candidate => candidate.Id == partId);
        return part is not null && part.Voices.Contains(clean, StringComparer.Ordinal) ? clean : null;
    }

    private static bool TryParseFingeringOverrides(string? text, out IReadOnlyDictionary<string, int[]> overrides)
    {
        var result = new Dictionary<string, int[]>(StringComparer.Ordinal);
        foreach (var rawLine in (text ?? string.Empty).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = rawLine.IndexOf('=');
            if (separator <= 0 || separator == rawLine.Length - 1)
            {
                overrides = result;
                return false;
            }

            var eventId = rawLine[..separator].Trim();
            var values = rawLine[(separator + 1)..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!IsExpectedEventId(eventId) || values.Length == 0 || values.Length > 10)
            {
                overrides = result;
                return false;
            }

            var fingers = new int[values.Length];
            for (var index = 0; index < values.Length; index++)
            {
                if (!int.TryParse(values[index], out var finger) || finger is < 1 or > 5)
                {
                    overrides = result;
                    return false;
                }
                fingers[index] = finger;
            }
            result[eventId] = fingers;
        }

        overrides = result;
        return true;
    }

    private static bool IsExpectedEventId(string value) =>
        value.Length is > 0 and <= 100 && value.StartsWith('m') && value.Contains("-v", StringComparison.Ordinal) && value.Contains("-n", StringComparison.Ordinal);

    private static string? ReadTitle(XDocument document) =>
        document.Descendants().FirstOrDefault(element => element.Name.LocalName == "work-title")?.Value
        ?? document.Descendants().FirstOrDefault(element => element.Name.LocalName == "movement-title")?.Value;

    private static bool HasValue(string? value) => !string.IsNullOrWhiteSpace(value);

    private static string? CleanOptional(string? value)
    {
        var clean = value?.Trim();
        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }

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
        if (read > maximumCharacters)
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
