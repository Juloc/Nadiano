namespace Nadiano.Web.Features.Practice;

public sealed record CreateSessionRequest(Guid SessionId, string LessonId, string ContentVersion, string Mode);

public sealed record CompleteSessionRequest(Guid AttemptId, int ResultSchemaVersion, string ResultJson, string NextActionCode);

public sealed record SessionCreatedResponse(Guid SessionId);

public sealed record AttemptResponse(Guid AttemptId, DateTimeOffset CompletedAtUtc, int ResultSchemaVersion, string ResultJson, string NextActionCode);