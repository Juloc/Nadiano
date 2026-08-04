namespace Nadiano.Web.Infrastructure.Persistence;

public static class DataPathResolver
{
    /// <summary>
    /// Resolves Nadiano:DataPath. Falls back to a repo-root "data" folder for local
    /// development (`dotnet run`), so no configuration is required to get started.
    /// In the container this is always set explicitly to "/data" (see docker-compose.yml).
    /// </summary>
    public static string Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredPath = configuration["Nadiano:DataPath"];

        var dataPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "data"))
            : configuredPath;

        Directory.CreateDirectory(dataPath);
        return dataPath;
    }
}