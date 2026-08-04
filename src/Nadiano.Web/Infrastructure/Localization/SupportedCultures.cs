using System.Globalization;

namespace Nadiano.Web.Infrastructure.Localization;

public static class SupportedCultures
{
    public const string Default = "de";

    public static readonly CultureInfo[] All =
    {
        new(Default),
        new("id"),
    };

    public static bool IsSupported(string? cultureName) =>
        All.Any(c => c.Name == cultureName);
}