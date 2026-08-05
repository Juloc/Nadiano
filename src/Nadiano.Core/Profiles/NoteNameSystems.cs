namespace Nadiano.Core.Profiles;

/// <summary>
/// Allowed values for ProfilePreferences.NoteNameSystem
/// (docs/PRODUCT_CONCEPT.md §6). Solfège and Indonesian numbered notation are
/// documented as later additions, not implemented yet.
/// </summary>
public static class NoteNameSystems
{
    public const string German = "german";
    public const string International = "international";

    public static readonly string[] All = [German, International];

    public static bool IsSupported(string value) => All.Contains(value);
}