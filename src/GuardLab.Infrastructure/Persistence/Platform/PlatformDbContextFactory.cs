using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GuardLab.Infrastructure.Persistence.Platform;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("GUARDLAB_PLATFORM_CONNECTION")
            ?? throw new InvalidOperationException("Set GUARDLAB_PLATFORM_CONNECTION to a migration connection using db_owner.");

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "platform"))
            .Options;

        return new PlatformDbContext(options);
    }
}
