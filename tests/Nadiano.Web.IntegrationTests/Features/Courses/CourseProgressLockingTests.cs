using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Nadiano.Web.Features.Practice;

namespace Nadiano.Web.IntegrationTests.Features.Courses;

/// <summary>
/// Exercises the WP-019 acceptance criterion that manual URL/API navigation
/// cannot start or complete a locked lesson, using the "progress-fixture"
/// course/lessons under Fixtures/content (lesson-b requires lesson-a).
/// </summary>
public class CourseProgressLockingTests : IClassFixture<ProgressWebApplicationFactory>
{
    private readonly ProgressWebApplicationFactory _factory;

    public CourseProgressLockingTests(ProgressWebApplicationFactory factory)
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
            if (header.StartsWith("nadiano-profile-id=", StringComparison.Ordinal))
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
    public async Task CreateSession_ForALessonWithUnmetPrerequisites_IsForbidden()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(Guid.NewGuid(), "lesson-b", "v1", "performance"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateSession_ForALessonWithNoPrerequisites_Succeeds()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(Guid.NewGuid(), "lesson-a", "v1", "performance"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateSession_ForALockedLesson_SucceedsOnceItsPrerequisiteIsCompleted()
    {
        using var client = CreateClient();

        // Complete lesson-a's single required run first.
        var lessonASessionId = Guid.NewGuid();
        var createLessonA = await client.PostAsJsonAsync(
            "/api/practice/sessions",
            new CreateSessionRequest(lessonASessionId, "lesson-a", "v1", "performance"));
        var cookie = ExtractProfileCookie(createLessonA);

        var completeLessonA = await client.SendAsync(WithCookie(
            HttpMethod.Post,
            $"/api/practice/sessions/{lessonASessionId}/complete",
            cookie,
            new CompleteSessionRequest(Guid.NewGuid(), 1, "{}", "well-done")));
        Assert.Equal(HttpStatusCode.OK, completeLessonA.StatusCode);

        // lesson-b was locked for this profile; it should be available now.
        var createLessonB = await client.SendAsync(WithCookie(
            HttpMethod.Post,
            "/api/practice/sessions",
            cookie,
            new CreateSessionRequest(Guid.NewGuid(), "lesson-b", "v1", "performance")));

        Assert.Equal(HttpStatusCode.OK, createLessonB.StatusCode);
    }
}
