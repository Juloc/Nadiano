namespace Nadiano.Web.Infrastructure.Content;

public static class ContentPathResolver
{
    /// <summary>
    /// Resolves Nadiano:ContentPath. Falls back to the repo-root "content"
    /// folder for local development, matching DataPathResolver's convention.
    /// The container copies content/ into the image and sets this explicitly
    /// (see Dockerfile) — bundled content is read-only at runtime.
    /// </summary>
    public static string Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredPath = configuration["Nadiano:ContentPath"];

        return string.IsNullOrWhiteSpace(configuredPath)
            ? Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "content"))
            : configuredPath;
    }
}