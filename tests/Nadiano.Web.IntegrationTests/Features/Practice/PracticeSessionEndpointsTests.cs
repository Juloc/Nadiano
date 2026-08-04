using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Nadiano.Web.Features.Practice;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.IntegrationTests.Features.Practice;

public class PracticeSessionEndpointsTests : IClassFixture<NadianoWebApplicationFactory>
{
    private readonly NadianoWebApplicationFactory _factory;

    public PracticeSessionEndpointsTests(NadianoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Cookies are managed manually via headers in these tests (to simulate different
    // profiles on one HttpClient), so the client's own automatic cookie jar is disabled —
    // otherwise it would silently attach the first response's real cookie to every
    // later request regardless of what this test explicitly sets.
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

    private static HttpRequestMessage WithCookie(HttpMethod method, string url, string? cookie, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    [Fact]
    public async Task CreateSession_AssignsAnAnonymousProfileCookie_OnFirstRequest()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(Guid.NewGuid(), "demo-lesson", "v1", "performance"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ExtractProfileCookie(response));
    }

    [Fact]
    public async Task CompleteSession_ResubmittingReturnsTheExistingResult_InsteadOfADuplicate()
    {
        using var client = CreateClient();
        var sessionId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(sessionId, "demo-lesson", "v1", "performance"));
        var cookie = ExtractProfileCookie(createResponse);

        var firstAttemptId = Guid.NewGuid();
        var firstComplete = await client.SendAsync(WithCookie(
            HttpMethod.Post,
            $"/api/practice/sessions/{sessionId}/complete",
            cookie,
            new CompleteSessionRequest(firstAttemptId, 1, """{"pitch":{"correctCount":4}}""", "well-done")));
        var firstResult = await firstComplete.Content.ReadFromJsonAsync<AttemptResponse>();

        // Resubmit with a DIFFERENT client-generated attemptId (simulating a retry after a lost response).
        var secondAttemptId = Guid.NewGuid();
        var secondComplete = await client.SendAsync(WithCookie(
            HttpMethod.Post,
            $"/api/practice/sessions/{sessionId}/complete",
            cookie,
            new CompleteSessionRequest(secondAttemptId, 1, """{"pitch":{"correctCount":0}}""", "repeat-section")));
        var secondResult = await secondComplete.Content.ReadFromJsonAsync<AttemptResponse>();

        Assert.Equal(HttpStatusCode.OK, secondComplete.StatusCode);
        Assert.Equal(firstResult!.AttemptId, secondResult!.AttemptId);
        Assert.Equal(firstResult.ResultJson, secondResult.ResultJson);
        Assert.Equal("well-done", secondResult.NextActionCode);
    }

    [Fact]
    public async Task GetSession_PreservesTheContentVersionRecordedAtCreation()
    {
        using var client = CreateClient();
        var sessionId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(sessionId, "demo-lesson", "content-v42", "wait"));
        var cookie = ExtractProfileCookie(createResponse);

        var getResponse = await client.SendAsync(WithCookie(HttpMethod.Get, $"/api/practice/sessions/{sessionId}", cookie));
        var json = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal("content-v42", json.GetProperty("contentVersion").GetString());
    }

    [Fact]
    public async Task OneProfile_CannotReadAnotherProfilesSession()
    {
        using var ownerClient = CreateClient();
        var sessionId = Guid.NewGuid();

        var createResponse = await ownerClient.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(sessionId, "demo-lesson", "v1", "performance"));
        var ownerCookie = ExtractProfileCookie(createResponse);
        Assert.NotNull(ownerCookie);

        var otherProfileCookie = $"{CurrentProfileAccessor.CookieName}={Guid.NewGuid()}";

        // A separate client stands in for a different household member's browser.
        using var otherClient = CreateClient();
        var getAsOther = await otherClient.SendAsync(WithCookie(HttpMethod.Get, $"/api/practice/sessions/{sessionId}", otherProfileCookie));
        var completeAsOther = await otherClient.SendAsync(WithCookie(
            HttpMethod.Post,
            $"/api/practice/sessions/{sessionId}/complete",
            otherProfileCookie,
            new CompleteSessionRequest(Guid.NewGuid(), 1, "{}", "well-done")));

        Assert.Equal(HttpStatusCode.NotFound, getAsOther.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, completeAsOther.StatusCode);

        // The owner can still read their own session — the isolation is per-profile, not a global lockout.
        var getAsOwner = await ownerClient.SendAsync(WithCookie(HttpMethod.Get, $"/api/practice/sessions/{sessionId}", ownerCookie));
        Assert.Equal(HttpStatusCode.OK, getAsOwner.StatusCode);
    }
}