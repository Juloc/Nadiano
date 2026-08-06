using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Nadiano.Web.Infrastructure.Persistence;

namespace Nadiano.Web.IntegrationTests.Features.Beta;

public sealed class BetaEndpointsTests : IClassFixture<NadianoWebApplicationFactory>
{
    private readonly NadianoWebApplicationFactory _factory;

    public BetaEndpointsTests(NadianoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Curriculum_ContainsRequiredBetaQuantities()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/beta/curriculum");
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(45, document.RootElement.GetProperty("lessons").GetArrayLength());
        Assert.Equal(100, document.RootElement.GetProperty("exercises").GetArrayLength());
    }

    [Fact]
    public async Task CourseMap_ExposesOneAvailableLessonAndLocksTheRemainder()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/Learn/Beta");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(45, Count(html, "data-status=\""));
        Assert.Equal(1, Count(html, "data-status=\"available\""));
        Assert.Equal(44, Count(html, "data-status=\"locked\""));
    }

    [Fact]
    public async Task EvidenceRetry_DoesNotCreateADuplicate()
    {
        using var client = _factory.CreateClient();
        var activityId = $"offline-retry-{Guid.NewGuid():N}";
        var request = new
        {
            activityId,
            activityKind = "reading-card",
            seed = 42,
            skillId = "reading.generated",
            expected = new { notes = new[] { 60 } },
            response = new { notes = new[] { 60 } },
            result = new { correct = true },
            outcome = "Good",
        };

        using var first = await client.PostAsJsonAsync("/api/beta/evidence", request);
        using var retry = await client.PostAsJsonAsync("/api/beta/evidence", request);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, retry.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NadianoDbContext>();
        Assert.Equal(1, await db.LearningEvidence.CountAsync(item => item.ActivityId == activityId));
    }

    [Fact]
    public async Task ServiceWorker_ExplicitlyExcludesPrivateLibraryRequests()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/service-worker.js");
        var script = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("url.pathname.startsWith(\"/api/library/\")", script, StringComparison.Ordinal);
        Assert.Contains("isPrivateRequest(url)", script, StringComparison.Ordinal);
    }

    private static int Count(string value, string fragment)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += fragment.Length;
        }

        return count;
    }
}