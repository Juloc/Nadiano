using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Nadiano.Web.Features.Practice;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.IntegrationTests.Features.Profiles;

public class ProfileExportEndpointsTests : IClassFixture<NadianoWebApplicationFactory>
{
    private readonly NadianoWebApplicationFactory _factory;

    public ProfileExportEndpointsTests(NadianoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    private static string? ExtractProfileCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
        {
            return null;
        }

        foreach (var header in headers)
        {
            if (header.StartsWith(CurrentProfileAccessor.CookieName + "=", StringComparison.Ordinal))
            {
                return header.Split(';')[0];
            }
        }

        return null;
    }

    private static HttpRequestMessage WithCookie(HttpMethod method, string url, string? cookie)
    {
        var request = new HttpRequestMessage(method, url);
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        return request;
    }

    [Fact]
    public async Task Export_IncludesTheProfilesCompletedPracticeSessions()
    {
        using var client = CreateClient();
        var sessionId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(sessionId, "demo-lesson", "v1", "performance"));
        var cookie = ExtractProfileCookie(createResponse);
        Assert.NotNull(cookie);

        var completeRequest = WithCookie(HttpMethod.Post, $"/api/practice/sessions/{sessionId}/complete", cookie);
        completeRequest.Content = JsonContent.Create(new CompleteSessionRequest(Guid.NewGuid(), 1, "{}", "well-done"));
        await client.SendAsync(completeRequest);

        var profileId = Guid.Parse(cookie[(cookie.IndexOf('=') + 1)..]);

        var exportResponse = await client.SendAsync(WithCookie(HttpMethod.Get, $"/api/profiles/{profileId}/export", cookie));
        Assert.Equal(HttpStatusCode.OK, exportResponse.StatusCode);

        var json = await exportResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(profileId, json.GetProperty("id").GetGuid());
        Assert.True(json.GetProperty("sessions").GetArrayLength() >= 1);
    }

    [Fact]
    public async Task Export_OfAnotherProfile_Returns404()
    {
        using var client = CreateClient();

        var indexResponse = await client.GetAsync("/Profiles");
        var cookie = ExtractProfileCookie(indexResponse);

        var response = await client.SendAsync(WithCookie(HttpMethod.Get, $"/api/profiles/{Guid.NewGuid()}/export", cookie));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
