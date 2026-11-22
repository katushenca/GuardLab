using GuardLab.Api;
using GuardLab.Api.Metrics;
using GuardLab.Application;
using GuardLab.Infrastructure;
using Prometheus;
using Prometheus.DotNetRuntime;

var builder = WebApplication.CreateBuilder(args);
var appVersion = builder.Configuration["APP_VERSION"] ?? "unknown";

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
    options.UseUtcTimestamp = true;
});

builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Mvc", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services
    .AddGuardLabApi(builder.Configuration)
    .AddGuardLabApplication()
    .AddGuardLabInfrastructure(builder.Configuration);
var app = builder.Build();
GuardLabMetrics.SetAppVersion(appVersion);

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-GuardLab-Version"] = appVersion;
        return Task.CompletedTask;
    });

    await next();
});

// подключают сбор метрик .NET Runtime
var runtimeMetrics = DotNetRuntimeStatsBuilder.Default()
    .WithThreadPoolStats(CaptureLevel.Informational)
    .StartCollecting();
app.Lifetime.ApplicationStopped.Register(runtimeMetrics.Dispose);

app.UseExceptionHandler();
app.UseRouting();
app.UseHttpMetrics(options =>
{
    // чтобы не увеличивать кардинальность из-за запросов с айдишниками
    options.AddCustomLabel("version", _ => appVersion);
    options.AddCustomLabel("route", context =>
        (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched");
});
app.UseMiddleware<RequestLoggingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseCors("LocalWeb");
}
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GuardLab API v1");
    options.RoutePrefix = "swagger";
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapMetrics();

app.Run();