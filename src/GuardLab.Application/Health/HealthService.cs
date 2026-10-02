namespace GuardLab.Application.Health;

public sealed class HealthService(IDatabaseHealthProbe databaseHealthProbe)
{
    public async Task<DatabaseHealthResult> CheckAsync(CancellationToken cancellationToken)
    {
        var isHealthy = await databaseHealthProbe.IsAvailableAsync(cancellationToken);
        return new DatabaseHealthResult(isHealthy);
    }
}
