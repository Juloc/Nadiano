namespace Nadiano.Core.Profiles;

/// <summary>
/// Independent per-profile preferences (docs/PRODUCT_CONCEPT.md §5,
/// docs/TECHNICAL_ARCHITECTURE.md §11 ProfilePreferences table). Note-name
/// system is deliberately separate from interface language
/// (docs/TECHNICAL_ARCHITECTURE.md §13).
/// </summary>
public sealed class ProfilePreferences
{
    public required Guid ProfileId { get; init; }

    public required string Language { get; set; }

    public required string NoteNameSystem { get; set; }

    public required int SessionLengthMinutes { get; set; }

    public string? PreferredMidiDeviceId { get; set; }

    public string? PreferredMidiDeviceName { get; set; }
}
