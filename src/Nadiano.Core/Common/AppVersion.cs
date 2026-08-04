using System.Reflection;

namespace Nadiano.Core.Common;

public static class AppVersion
{
    public static string Current { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var informationalVersion = typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return "0.0.0-dev";
        }

        var plusIndex = informationalVersion.IndexOf('+');
        return plusIndex >= 0 ? informationalVersion[..plusIndex] : informationalVersion;
    }
}