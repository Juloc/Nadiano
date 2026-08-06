using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

using Nadiano.Core.Content;
using Nadiano.Core.Content.Validation;
using Nadiano.Web.Features.Content;
using Nadiano.Web.Features.Diagnostics;
using Nadiano.Web.Features.Library;
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
builder.Services.AddSingleton(new PrivateLibraryStorage(dataPath));

builder.Services.AddScoped<CurrentProfileAccessor>();
builder.Services.AddScoped<CourseProgressService>();
builder.Services.AddScoped<ProgressSummaryService>();
builder.Services.AddScoped<PrivateLibraryService>();

var app = builder.Build();

using (var startupScope = app.Services.CreateScope())
{
    var db = startupScope.ServiceProvider.GetRequiredService<NadianoDbContext>();
    try
    {
        db.Database.Migrate();
        startupScope.ServiceProvider.GetRequiredService<PrivateLibraryService>()
            .RemoveAbandonedStagingData(TimeSpan.FromHours(24));
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(
            ex,
            "Database migration or private-library cleanup failed for data path '{DataPath}'. Nadiano will not start until this is resolved.",
            dataPath);
        throw;
    }
}

ValidateBundledContentOrThrow(app.Services.GetRequiredService<BundledContentRepository>(), app.Logger);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

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
app.MapReleaseDiagnosticsEndpoints();
app.MapPrivateLibraryEndpoints();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

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

public partial class Program;
