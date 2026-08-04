using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Nadiano.Web.IntegrationTests;

/// <summary>Runs the real app against an isolated temp SQLite file per factory instance, never the developer's local data/ folder.</summary>
public class NadianoWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DataPath { get; } = Path.Combine(Path.GetTempPath(), $"nadiano-test-data-{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(DataPath);
        builder.UseSetting("Nadiano:DataPath", DataPath);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(DataPath))
        {
            try
            {
                Directory.Delete(DataPath, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort cleanup; a lingering temp folder does not fail the test run.
            }
        }
    }
}