using GuardLab.Application.Health;
using Npgsql;

namespace GuardLab.Infrastructure.Persistence;

public sealed class NpgsqlDatabaseHealthProbe(NpgsqlDataSource dataSource) : IDatabaseHealthProbe
{
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return true;
        }
        catch (NpgsqlException)
        {
            return false;
        }
    }
}
