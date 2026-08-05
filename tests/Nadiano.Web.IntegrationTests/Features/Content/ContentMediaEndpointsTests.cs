using System.Net;

namespace Nadiano.Web.IntegrationTests.Features.Content;

/// <summary>
/// WP-020: technique media is only ever served for paths a lesson manifest
/// itself declares (docs/CONTENT_MODEL.md §16) — using the "lesson-a" fixture
/// under Fixtures/content, which declares one top-view video with a poster.
/// </summary>
public class ContentMediaEndpointsTests : IClassFixture<ProgressWebApplicationFactory>
{
    private readonly ProgressWebApplicationFactory _factory;

    public ContentMediaEndpointsTests(ProgressWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMedia_ForADeclaredVideoPath_ServesItWithTheVideoContentType()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/content/lessons/lesson-a/files/media/technique-top.webm");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("video/webm", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMedia_ForTheDeclaredPoster_ServesItWithTheImageContentType()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/content/lessons/lesson-a/files/media/technique-top.webp");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/webp", response.Content.Headers.ContentType?.MediaType);
    }


    [Fact]
    public async Task GetMedia_ForDeclaredNotationAndGeneratedEvents_ServesBothFiles()
    {
        using var client = _factory.CreateClient();

        var scoreResponse = await client.GetAsync("/api/content/lessons/lesson-b/files/score.musicxml");
        var eventsResponse = await client.GetAsync("/api/content/lessons/lesson-b/files/expected-events.json");

        Assert.Equal(HttpStatusCode.OK, scoreResponse.StatusCode);
        Assert.Equal("application/vnd.recordare.musicxml+xml", scoreResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.OK, eventsResponse.StatusCode);
        Assert.Equal("application/json", eventsResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMedia_ForAPathTheManifestDidNotDeclare_IsNotFound()
    {
        using var client = _factory.CreateClient();

        // lesson.json exists on disk but was never declared as a technique view/poster.
        var response = await client.GetAsync("/api/content/lessons/lesson-a/files/lesson.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMedia_ForALessonWithNoTechniqueMetadata_IsNotFound()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/content/lessons/lesson-b/files/media/technique-top.webm");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMedia_ForAnUnknownLesson_IsNotFound()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/content/lessons/unknown-lesson/files/media/technique-top.webm");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
