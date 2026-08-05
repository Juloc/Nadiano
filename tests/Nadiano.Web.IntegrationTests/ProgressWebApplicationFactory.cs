using Microsoft.AspNetCore.Hosting;

namespace Nadiano.Web.IntegrationTests;

/// <summary>Same isolated temp database as NadianoWebApplicationFactory, but with the
/// Fixtures/content course/lesson tree bundled instead of the (empty) real content/
/// folder, so lock-enforcement tests have real prerequisites to exercise.</summary>
public class ProgressWebApplicationFactory : NadianoWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Nadiano:ContentPath", Path.Combine(AppContext.BaseDirectory, "Fixtures", "content"));
    }
}
