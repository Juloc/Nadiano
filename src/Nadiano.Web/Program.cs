using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Validation;
using Nadiano.Web.Features.Content;
using Nadiano.Web.Features.Practice;
using Nadiano.Web.Features.Profiles;
using Nadiano.Web.Features.Progress;
using Nadiano.Web.Infrastructure.Content;
using Nadiano.Web.Infrastructure.Courses;
using Nadiano.Web.Infrastructure.Localization;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture: SupportedCultures.Default, uiCulture: SupportedCultures.Default);
    options.SupportedCultures = SupportedCultures.All;
    options.SupportedUICultures = SupportedCultures.All;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

var dataPath = DataPathResolver.Resolve(builder.Configuration, builder.Environment);
var databasePath = Path.Combine(dataPath, "nadiano.db");
builder.Services.AddDbContext<NadianoDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));

var contentPath = ContentPathResolver.Resolve(builder.Configuration, builder.Environment);
builder.Services.AddSingleton(new BundledContentRepository(contentPath));
builder.Services.AddSingleton<ContentCatalogue>();

builder.Services.AddScoped<CurrentProfileAccessor>();
builder.Services.AddScoped<CourseProgressService>();

var app = builder.Build();

using (var startupScope = app.Services.CreateScope())
{
    var db = startupScope.ServiceProvider.GetRequiredService<NadianoDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(
            ex,
            "Database migration failed for data path '{DataPath}'. Nadiano will not start until this is resolved.",
            dataPath);
        throw;
    }
}

ValidateBundledContentOrThrow(app.Services.GetRequiredService<BundledContentRepository>(), app.Logger);

// Nadiano is served as plain HTTP behind an external reverse proxy in every
// environment (see docs/TECHNICAL_ARCHITECTURE.md §15/§19). Web MIDI treats
// http://localhost as a secure context, so no local HTTPS/dev-cert is needed.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Only the HTML pages get the friendly 404 page. JSON API endpoints must keep
// returning their real (empty-body) status code, or a client-side 404 check
// like the practice workspace's profile-isolation logic would see an HTML
// page instead (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-017).
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found"));

app.UseRequestLocalization();

app.UseRouting();

app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.MapPracticeSessionEndpoints();
app.MapProfileExportEndpoints();
app.MapSelfCheckEndpoints();
app.MapContentMediaEndpoints();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Bundled content is validated on every startup, not only in CI
// (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-011 step 6). An empty content root
// is treated as "not authored yet" rather than a failure, so the app keeps
// running through the phases before real content lands (WP-021).
static void ValidateBundledContentOrThrow(BundledContentRepository repository, ILogger logger)
{
    var hasAnyCourses = repository.DiscoverCourseIds().Count > 0;
    var hasSkillCatalogue = File.Exists(repository.SkillsCataloguePath);

    if (!hasAnyCourses && !hasSkillCatalogue)
    {
        logger.LogWarning("No bundled content found under '{ContentRoot}' yet.", repository.ContentRoot);
        return;
    }

    var result = new ContentValidator(repository).ValidateAll();
    if (result.IsValid)
    {
        return;
    }

    foreach (var issue in result.Issues)
    {
        logger.LogError("Content validation issue: {Issue}", issue);
    }

    throw new InvalidOperationException(
        $"Bundled content under '{repository.ContentRoot}' failed validation with {result.Issues.Count} issue(s). Nadiano will not start until this is resolved.");
}

// Gives WebApplicationFactory<Program> in the integration test project something public to reference.
public partial class Program;