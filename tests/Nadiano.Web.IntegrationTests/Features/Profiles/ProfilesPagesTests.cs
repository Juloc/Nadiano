using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;

using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.IntegrationTests.Features.Profiles;

public partial class ProfilesPagesTests : IClassFixture<NadianoWebApplicationFactory>
{
    private readonly NadianoWebApplicationFactory _factory;

    public ProfilesPagesTests(NadianoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false, AllowAutoRedirect = false });

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();

    private static string? ExtractCookie(HttpResponseMessage response, string namePrefix)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
        {
            return null;
        }

        foreach (var header in headers)
        {
            if (header.StartsWith(namePrefix, StringComparison.Ordinal))
            {
                return header.Split(';')[0];
            }
        }

        return null;
    }

    private static string CombineCookies(params string?[] cookies) =>
        string.Join("; ", cookies.Where(c => c is not null));

    /// <summary>GETs a page to obtain a fresh profile cookie + antiforgery cookie/token pair for a subsequent POST.</summary>
    private static async Task<(string ProfileCookie, string AntiforgeryCookie, string Token, string Html)> GetPageWithAntiforgeryAsync(
        HttpClient client, string url, string? existingProfileCookie = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (existingProfileCookie is not null)
        {
            request.Headers.Add("Cookie", existingProfileCookie);
        }

        var response = await client.SendAsync(request);
        var html = await response.Content.ReadAsStringAsync();

        var profileCookie = ExtractCookie(response, CurrentProfileAccessor.CookieName + "=") ?? existingProfileCookie
            ?? throw new InvalidOperationException("No profile cookie available.");
        var antiforgeryCookie = ExtractCookie(response, ".AspNetCore.Antiforgery.")
            ?? throw new InvalidOperationException("No antiforgery cookie returned.");
        var token = AntiforgeryTokenRegex().Match(html) is { Success: true } match
            ? match.Groups[1].Value
            : throw new InvalidOperationException($"No antiforgery token found in page:\n{html}");

        return (profileCookie, antiforgeryCookie, token, html);
    }

    [Fact]
    public async Task Index_AutoCreatesADefaultProfile_OnFirstVisit()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/Profiles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ExtractCookie(response, CurrentProfileAccessor.CookieName + "="));
    }

    [Fact]
    public async Task CreatingAProfile_ThenSelectingIt_SwitchesTheCurrentProfileCookie()
    {
        using var client = CreateClient();

        var (profileCookie, antiforgeryCookie, token, _) = await GetPageWithAntiforgeryAsync(client, "/Profiles");

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/Profiles?handler=Create")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["NewProfileName"] = "Zweites Profil",
                ["__RequestVerificationToken"] = token,
            }),
        };
        createRequest.Headers.Add("Cookie", CombineCookies(profileCookie, antiforgeryCookie));

        var createResponse = await client.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.Redirect, createResponse.StatusCode);
        var newProfileCookie = ExtractCookie(createResponse, CurrentProfileAccessor.CookieName + "=");
        Assert.NotNull(newProfileCookie);
        Assert.NotEqual(profileCookie, newProfileCookie);

        var (_, _, _, indexHtml) = await GetPageWithAntiforgeryAsync(client, "/Profiles", newProfileCookie);
        Assert.Contains("Zweites Profil", indexHtml);
    }

    [Fact]
    public async Task Edit_UnknownProfile_Returns404()
    {
        using var client = CreateClient();

        var response = await client.GetAsync($"/Profiles/Edit/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Edit_RenamingTheCurrentProfile_PersistsTheNewNameAndPreferences()
    {
        using var client = CreateClient();

        var indexResponse = await client.GetAsync("/Profiles");
        var profileCookie = ExtractCookie(indexResponse, CurrentProfileAccessor.CookieName + "=")!;
        var profileId = ProfileIdFromCookie(profileCookie);

        var (_, antiforgeryCookie, token, _) = await GetPageWithAntiforgeryAsync(client, $"/Profiles/Edit/{profileId}", profileCookie);

        var editRequest = new HttpRequestMessage(HttpMethod.Post, $"/Profiles/Edit/{profileId}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Id"] = profileId.ToString(),
                ["Name"] = "Umbenannt",
                ["Language"] = "id",
                ["NoteNameSystem"] = "international",
                ["SessionLengthMinutes"] = "30",
                ["__RequestVerificationToken"] = token,
            }),
        };
        editRequest.Headers.Add("Cookie", CombineCookies(profileCookie, antiforgeryCookie));

        var editResponse = await client.SendAsync(editRequest);
        Assert.Equal(HttpStatusCode.Redirect, editResponse.StatusCode);

        var indexAfter = await client.SendAsync(WithCookie(HttpMethod.Get, "/Profiles", profileCookie));
        var htmlAfter = await indexAfter.Content.ReadAsStringAsync();
        Assert.Contains("Umbenannt", htmlAfter);
    }

    [Fact]
    public async Task Delete_RemovesTheProfile_AndClearsTheCookieWhenItWasCurrent()
    {
        using var client = CreateClient();

        var indexResponse = await client.GetAsync("/Profiles");
        var profileCookie = ExtractCookie(indexResponse, CurrentProfileAccessor.CookieName + "=")!;
        var profileId = ProfileIdFromCookie(profileCookie);

        var (_, antiforgeryCookie, token, _) = await GetPageWithAntiforgeryAsync(client, $"/Profiles/Delete/{profileId}", profileCookie);

        var deleteRequest = new HttpRequestMessage(HttpMethod.Post, $"/Profiles/Delete/{profileId}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Id"] = profileId.ToString(),
                ["__RequestVerificationToken"] = token,
            }),
        };
        deleteRequest.Headers.Add("Cookie", CombineCookies(profileCookie, antiforgeryCookie));

        var deleteResponse = await client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.Redirect, deleteResponse.StatusCode);

        var editAfterDelete = await client.SendAsync(WithCookie(HttpMethod.Get, $"/Profiles/Edit/{profileId}", profileCookie));
        Assert.Equal(HttpStatusCode.NotFound, editAfterDelete.StatusCode);
    }

    private static HttpRequestMessage WithCookie(HttpMethod method, string url, string cookie)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Cookie", cookie);
        return request;
    }

    private static Guid ProfileIdFromCookie(string cookie) =>
        Guid.Parse(cookie[(cookie.IndexOf('=') + 1)..]);
}