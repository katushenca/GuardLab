using System.Text.Json;
using GuardLab.Application;
using GuardLab.Infrastructure;
using GuardLab.Infrastructure.Persistence.Platform;
using GuardLab.Infrastructure.Persistence.Sandbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

var stage = "configuration";
try
{
    var configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(configuration);
    services.AddGuardLabApplication();
    services.AddGuardLabInfrastructure(configuration);

    await using var provider = services.BuildServiceProvider();
    await using var scope = provider.CreateAsyncScope();

    var platform = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    var sandbox = scope.ServiceProvider.GetRequiredService<SandboxDbContext>();

    stage = "platform_migrations";
    WriteLog("Information", "db.migration.started", "Applying database migrations", stage, ("database", "platform"));
    await platform.Database.MigrateAsync();
    WriteLog("Information", "db.migration.completed", "Database migrations applied", stage, ("database", "platform"));

    stage = "sandbox_migrations";
    WriteLog("Information", "db.migration.started", "Applying database migrations", stage, ("database", "sandbox"));
    await sandbox.Database.MigrateAsync();
    WriteLog("Information", "db.migration.completed", "Database migrations applied", stage, ("database", "sandbox"));

    stage = "admin_seed";
    WriteLog("Information", "db.seed.started", "Seeding administrator account", stage);
    var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminUserSeeder>();
    await adminSeeder.SeedAsync();
    WriteLog("Information", "db.seed.completed", "Administrator account seeded", stage);

    stage = "sql_seed";
    var platformConnection = configuration.GetConnectionString("Platform")
                             ?? throw new InvalidOperationException("Platform connection string is missing.");

    await using var connection = new NpgsqlConnection(platformConnection);
    await connection.OpenAsync();

    foreach (var resourceName in new[]
             {
                 "GuardLab.DbMigrator.Seeds.03-seed.sql",
                 "GuardLab.DbMigrator.Seeds.04-idor-guids.sql"
             })
    {
        var scriptName = Path.GetFileName(resourceName);
        stage = $"sql_seed:{scriptName}";
        WriteLog("Information", "db.seed.started", "Applying seed script", stage, ("script", scriptName));

        await using var script = typeof(Program).Assembly.GetManifestResourceStream(resourceName)
                                 ?? throw new InvalidOperationException("Embedded seed script is missing.");
        using var reader = new StreamReader(script);
        await using var command = new NpgsqlCommand(await reader.ReadToEndAsync(), connection);
        await command.ExecuteNonQueryAsync();

        WriteLog("Information", "db.seed.completed", "Seed script applied", stage, ("script", scriptName));
    }

    stage = "completed";
    WriteLog("Information", "db-migrator.completed", "Database initialization completed", stage);
}
catch (Exception exception)
{
    WriteLog(
        "Error",
        "db-migrator.failed",
        "Database initialization failed",
        stage,
        ("exceptionType", exception.GetType().Name));
    Environment.ExitCode = 1;
}

static void WriteLog(
    string logLevel,
    string eventName,
    string message,
    string stage,
    params (string Key, object? Value)[] fields)
{
    var record = new Dictionary<string, object?>
    {
        ["Timestamp"] = DateTimeOffset.UtcNow,
        ["LogLevel"] = logLevel,
        ["Category"] = "GuardLab.DbMigrator",
        ["Message"] = message,
        ["EventName"] = eventName,
        ["service.name"] = "guardlab-db-migrator",
        ["Stage"] = stage
    };

    foreach (var (key, value) in fields)
        record[key] = value;

    Console.WriteLine(JsonSerializer.Serialize(record));
}