using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nadiano.Web.Infrastructure.Persistence;

public class DatabaseHealthCheck(NadianoDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("SQLite database is reachable.")
            : HealthCheckResult.Unhealthy("SQLite database is not reachable.");
    }
}