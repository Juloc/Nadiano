using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Common;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.Features.Diagnostics;

/// <summary>
/// Exposes release and schema versions without profile data, lesson prose or
/// raw practice history. This endpoint is safe to include in support reports.
/// </summary>
public static class ReleaseDiagnosticsEndpoints
{
    public static void MapReleaseDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/diagnostics/version", GetVersionAsync);
    }

    private static async Task<IResult> GetVersionAsync(
        NadianoDbContext db,
        ContentCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync(cancellationToken);
        var courses = catalogue.CourseIds
            .Select(courseId =>
            {
                catalogue.TryGetCourse(courseId, out var course, out _);
                return new
                {
                    id = course.Id,
                    version = course.Version,
                    schemaVersion = course.SchemaVersion,
                };
            })
            .OrderBy(course => course.id, StringComparer.Ordinal)
            .ToArray();

        return Results.Ok(new
        {
            applicationVersion = AppVersion.Current,
            database = new
            {
                provider = db.Database.ProviderName,
                latestMigration = appliedMigrations.LastOrDefault() ?? "none",
            },
            content = new
            {
                courseCount = courses.Length,
                courses,
            },
        });
    }
}