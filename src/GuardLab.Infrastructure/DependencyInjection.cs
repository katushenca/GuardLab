using GuardLab.Application.Health;
using Microsoft.EntityFrameworkCore;
using GuardLab.Application.Abstractions;
using GuardLab.Infrastructure.Persistence;
using GuardLab.Infrastructure.Persistence.Sandbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using GuardLab.Infrastructure.Persistence.Platform;

namespace GuardLab.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGuardLabInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Platform")
            ?? throw new InvalidOperationException("ConnectionStrings:Platform is missing.");

        services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "platform")));
        var sandboxConnectionString = configuration.GetConnectionString("Sandbox")
            ?? throw new InvalidOperationException("ConnectionStrings:Sandbox is missing.");
        services.AddDbContext<SandboxDbContext>(options =>
            options.UseNpgsql(sandboxConnectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "sandbox")));
        services.AddScoped<IDatabaseHealthProbe, NpgsqlDatabaseHealthProbe>();
        services.AddScoped<ILabRepository, PostgresLabRepository>();
        services.AddScoped<IAdminUserRepository, PostgresAdminUserRepository>();
        services.AddScoped<AdminUserSeeder>();
        services.AddScoped<ISandboxKeyRepository, PostgresSandboxKeyRepository>();
        return services;
    }
}
