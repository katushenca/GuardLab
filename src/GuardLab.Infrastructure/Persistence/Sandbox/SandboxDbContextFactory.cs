using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GuardLab.Infrastructure.Persistence.Sandbox;

public sealed class SandboxDbContextFactory : IDesignTimeDbContextFactory<SandboxDbContext>
{
    public SandboxDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("GUARDLAB_SANDBOX_CONNECTION")
            ?? throw new InvalidOperationException("Set GUARDLAB_SANDBOX_CONNECTION to a migration connection using db_owner.");

        var options = new DbContextOptionsBuilder<SandboxDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "sandbox"))
            .Options;

        return new SandboxDbContext(options);
    }
}
