using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using Nadiano.Web.Features.Progress;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.IntegrationTests.Features.Progress;

/// <summary>
/// WP-020: self-check answers are stored as learner evidence for the current
/// profile only — never scored, never used to unlock content.
/// </summary>
public class SelfCheckEndpointsTests : IClassFixture<NadianoWebApplicationFactory>
{
    private readonly NadianoWebApplicationFactory _factory;

    public SelfCheckEndpointsTests(NadianoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    private static string? ExtractProfileCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return null;
        }

        foreach (var header in setCookieHeaders)
        {
            if (header.StartsWith(CurrentProfileAccessor.CookieName + "=", StringComparison.Ordinal))
            {
                return header.Split(';')[0];
            }
        }

        return null;
    }

    [Fact]
    public async Task RecordSelfCheck_PersistsTheAnswerForTheRequestingProfile()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/progress/self-checks",
            new RecordSelfCheckRequest("lesson-a", "technique.thumb-loose", true));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profileCookie = ExtractProfileCookie(response);
        Assert.NotNull(profileCookie);
        var profileId = Guid.Parse(profileCookie![(CurrentProfileAccessor.CookieName.Length + 1)..]);

        var body = await response.Content.ReadFromJsonAsync<RecordSelfCheckResponse>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NadianoDbContext>();
        var stored = await db.SkillEvidence.SingleAsync(e => e.Id == body!.EvidenceId);

        Assert.Equal(profileId, stored.ProfileId);
        Assert.Equal("lesson-a", stored.LessonId);
        Assert.Equal("technique.thumb-loose", stored.SkillId);
        Assert.True(stored.SelfReportedSuccess);
    }

    [Fact]
    public async Task RecordSelfCheck_AllowsANegativeAnswer()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/progress/self-checks",
            new RecordSelfCheckRequest("lesson-a", "body.wrist-neutral", false));
        var body = await response.Content.ReadFromJsonAsync<RecordSelfCheckResponse>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NadianoDbContext>();
        var stored = await db.SkillEvidence.SingleAsync(e => e.Id == body!.EvidenceId);

        Assert.False(stored.SelfReportedSuccess);
    }
}
