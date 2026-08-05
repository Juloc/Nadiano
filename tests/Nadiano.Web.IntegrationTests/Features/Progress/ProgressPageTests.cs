using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Nadiano.Core.Practice;
using Nadiano.Core.Profiles;
using Nadiano.Core.Progress;
using Nadiano.Web.Infrastructure.Persistence;
using Nadiano.Web.Infrastructure.Profiles;

namespace Nadiano.Web.IntegrationTests.Features.Progress;

public class ProgressPageTests : IClassFixture<ProgressWebApplicationFactory>
{
    private readonly ProgressWebApplicationFactory _factory;

    public ProgressPageTests(ProgressWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Progress_ShowsProfileScopedEvidence_WithCautiousTrends_InGermanAndIndonesian()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false,
        });

        var firstResponse = await client.GetAsync("/Progress");
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var profileCookie = ExtractProfileCookie(firstResponse);
        Assert.NotNull(profileCookie);
        var profileId = Guid.Parse(profileCookie![(CurrentProfileAccessor.CookieName.Length + 1)..]);

        await SeedEvidenceAsync(profileId);

        var germanRequest = WithCookie("/Progress", profileCookie);
        var germanResponse = await client.SendAsync(germanRequest);
        var germanHtml = await germanResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, germanResponse.StatusCode);
        Assert.Contains("Lektion A", germanHtml);
        Assert.Contains("6 ausgewerteten Versuchen", germanHtml);
        Assert.Contains("Die letzten drei Versuche waren besser", germanHtml);
        Assert.Contains("fällig", germanHtml);
        Assert.DoesNotContain("foreign-lesson", germanHtml);
        Assert.DoesNotContain("Progress.", germanHtml);

        var indonesianRequest = WithCookie("/Progress", profileCookie);
        indonesianRequest.Headers.AcceptLanguage.ParseAdd("id");
        var indonesianResponse = await client.SendAsync(indonesianRequest);
        var indonesianHtml = await indonesianResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, indonesianResponse.StatusCode);
        Assert.Contains("Kemajuan", indonesianHtml);
        Assert.Contains("Pelajaran A", indonesianHtml);
        Assert.Contains("Tiga percobaan terakhir lebih baik", indonesianHtml);
        Assert.DoesNotContain("Progress.", indonesianHtml);
    }

    private async Task SeedEvidenceAsync(Guid profileId)
    {
        var now = DateTimeOffset.UtcNow;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NadianoDbContext>();

        for (var index = 0; index < 6; index++)
        {
            var completedAt = now.AddDays(-2).AddMinutes(index);
            var correct = index < 3 ? 2 : 4;
            var timingBands = index < 3
                ? "[{\"deviationMs\":0,\"band\":\"onTime\"},{\"deviationMs\":180,\"band\":\"late\"}]"
                : "[{\"deviationMs\":0,\"band\":\"onTime\"},{\"deviationMs\":20,\"band\":\"onTime\"}]";
            var resultJson = $$"""
                {
                  "pitch": {
                    "totalExpected": 4,
                    "correctCount": {{correct}},
                    "omittedCount": {{4 - correct}},
                    "additionCount": 0
                  },
                  "onset": {
                    "deviations": {{timingBands}}
                  }
                }
                """;

            var sessionId = Guid.NewGuid();
            db.PracticeSessions.Add(new PracticeSessionRecord
            {
                Id = sessionId,
                ProfileId = profileId,
                LessonId = "lesson-a",
                ContentVersion = "0.1.0",
                Mode = "performance",
                StartedAtUtc = completedAt.AddMinutes(-1),
                Attempt = new PracticeAttemptRecord
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    CompletedAtUtc = completedAt,
                    ResultSchemaVersion = 1,
                    ResultJson = resultJson,
                    NextActionCode = index < 3 ? "repeat-section" : "well-done",
                },
            });
        }

        db.LessonProgress.Add(new LessonProgressRecord
        {
            ProfileId = profileId,
            CourseId = "progress-fixture",
            LessonId = "lesson-a",
            CompletedAtUtc = now.AddDays(-2),
        });

        for (var index = 0; index < 6; index++)
        {
            db.SkillEvidence.Add(new SkillEvidenceRecord
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                LessonId = "lesson-a",
                SkillId = "technique.fixture",
                SelfReportedSuccess = index >= 3,
                RecordedAtUtc = now.AddDays(-2).AddMinutes(index),
            });
        }

        var foreignProfileId = Guid.NewGuid();
        db.LearnerProfiles.Add(new LearnerProfile
        {
            Id = foreignProfileId,
            Name = "Other profile",
            CreatedAtUtc = now,
        });
        db.ProfilePreferences.Add(new ProfilePreferences
        {
            ProfileId = foreignProfileId,
            Language = "de",
            NoteNameSystem = NoteNameSystems.German,
            SessionLengthMinutes = 20,
        });

        var foreignSessionId = Guid.NewGuid();
        db.PracticeSessions.Add(new PracticeSessionRecord
        {
            Id = foreignSessionId,
            ProfileId = foreignProfileId,
            LessonId = "foreign-lesson",
            ContentVersion = "0.1.0",
            Mode = "performance",
            StartedAtUtc = now,
            Attempt = new PracticeAttemptRecord
            {
                Id = Guid.NewGuid(),
                SessionId = foreignSessionId,
                CompletedAtUtc = now,
                ResultSchemaVersion = 1,
                ResultJson = "{}",
                NextActionCode = "well-done",
            },
        });

        await db.SaveChangesAsync();
    }

    private static HttpRequestMessage WithCookie(string url, string cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Cookie", cookie);
        return request;
    }

    private static string? ExtractProfileCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return null;
        }

        return setCookieHeaders
            .FirstOrDefault(header => header.StartsWith(CurrentProfileAccessor.CookieName + "=", StringComparison.Ordinal))?
            .Split(';')[0];
    }
}