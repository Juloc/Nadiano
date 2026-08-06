using System.Net;
using System.Text.Json;

namespace Nadiano.Web.IntegrationTests.Features.Diagnostics;

public class ReleaseDiagnosticsEndpointsTests : IClassFixture<ProgressWebApplicationFactory>
{
    private readonly ProgressWebApplicationFactory _factory;

    public ReleaseDiagnosticsEndpointsTests(ProgressWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task VersionDiagnostics_ReturnsApplicationDatabaseAndContentVersions_WithoutProfileData()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/version");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("0.2.0-beta.1", root.GetProperty("applicationVersion").GetString());
        Assert.NotEqual("none", root.GetProperty("database").GetProperty("latestMigration").GetString());
        Assert.Equal(1, root.GetProperty("content").GetProperty("courseCount").GetInt32());

        var course = root.GetProperty("content").GetProperty("courses")[0];
        Assert.Equal("progress-fixture", course.GetProperty("id").GetString());
        Assert.Equal("0.1.0", course.GetProperty("version").GetString());
        Assert.Equal(1, course.GetProperty("schemaVersion").GetInt32());

        Assert.DoesNotContain("profile", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("practice", json, StringComparison.OrdinalIgnoreCase);
    }
}