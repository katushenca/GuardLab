using GuardLab.Application;
using GuardLab.Infrastructure;
using GuardLab.Infrastructure.Persistence.Platform;
using GuardLab.Infrastructure.Persistence.Sandbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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

await platform.Database.MigrateAsync();

await sandbox.Database.MigrateAsync();

var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminUserSeeder>();
await adminSeeder.SeedAsync();

var platformConnection = configuration.GetConnectionString("Platform")
                         ?? throw new InvalidOperationException();

await using var connection = new NpgsqlConnection(platformConnection);
await connection.OpenAsync();

foreach (var resourceName in new[]
         {
             "GuardLab.DbMigrator.Seeds.03-seed.sql",
             "GuardLab.DbMigrator.Seeds.04-idor-guids.sql"
         })
{
    await using var script = typeof(Program).Assembly.GetManifestResourceStream(resourceName)
                             ?? throw new InvalidOperationException();
    using var reader = new StreamReader(script);
    await using var command = new NpgsqlCommand(await reader.ReadToEndAsync(), connection);
    await command.ExecuteNonQueryAsync();
}

Console.WriteLine("Database initialization completed.");