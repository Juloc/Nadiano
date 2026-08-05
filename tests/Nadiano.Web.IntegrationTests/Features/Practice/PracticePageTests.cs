using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Nadiano.Web.Features.Practice;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.IntegrationTests.Features.Practice;

public class PracticePageTests : IClassFixture<ProgressWebApplicationFactory>
{
    private readonly ProgressWebApplicationFactory _factory;

    public PracticePageTests(ProgressWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PracticeWithoutLesson_ShowsLocalizedSelectionState()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/Practice");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Wähle zuerst eine Lektion mit Notenübung aus.", html);
        Assert.DoesNotContain("practice-workspace", html);
    }

    [Fact]
    public async Task PracticeForUnlockedNotationLesson_UsesManifestDataAndDeclaredFiles()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var lessonASessionId = Guid.NewGuid();
        var createLessonA = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(lessonASessionId, "lesson-a", "0.1.0", "dry"));
        var cookie = ExtractProfileCookie(createLessonA);
        Assert.NotNull(cookie);

        var completeLessonA = await client.SendAsync(WithCookie(
            HttpMethod.Post,
            $"/api/practice/sessions/{lessonASessionId}/complete",
            cookie,
            new CompleteSessionRequest(Guid.NewGuid(), 1, "{}", "well-done")));
        Assert.Equal(HttpStatusCode.OK, completeLessonA.StatusCode);

        var response = await client.SendAsync(WithCookie(HttpMethod.Get, "/Practice?lessonId=lesson-b", cookie));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("id=\"practice-workspace\"", html);
        Assert.Contains("data-lesson-id=\"lesson-b\"", html);
        Assert.Contains("data-score-url=\"/api/content/lessons/lesson-b/files/score.musicxml\"", html);
        Assert.Contains("data-expected-events-url=\"/api/content/lessons/lesson-b/files/expected-events.json\"", html);
        Assert.Contains("data-target-tempo=\"66\"", html);
        Assert.Contains("value=\"wait\"", html);
        Assert.Contains("value=\"performance\"", html);
        Assert.DoesNotContain("demo-notation-fixture", html);
    }

    [Fact]
    public async Task PracticeForLockedLesson_RedirectsToLearnMap()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Practice?lessonId=lesson-b");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Learn", response.Headers.Location?.OriginalString);
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

    private static string? ExtractProfileCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
        {
            return null;
        }

        return headers
            .FirstOrDefault(header => header.StartsWith(CurrentProfileAccessor.CookieName + "=", StringComparison.Ordinal))?
            .Split(';')[0];
    }
}