using System.Diagnostics;

namespace GuardLab.Api;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.Request.Headers.TryGetValue("X-Trace-Id", out var incoming)
                      && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");

        context.Response.Headers["X-Trace-Id"] = traceId;
        var stopwatch = Stopwatch.StartNew();
        var routeTemplate = context.GetEndpoint() is RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText ?? "<unmatched>"
            : "<unmatched>";

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["service.name"] = "guardlab-api",
                   ["traceId"] = traceId
               }))
        {
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();

                var isSuccessfulHealthCheck = routeTemplate == "api/health"
                                              && context.Response.StatusCode < StatusCodes.Status400BadRequest;

                if (!isSuccessfulHealthCheck)
                {
                    var logLevel = context.Response.StatusCode >= StatusCodes.Status500InternalServerError
                        ? LogLevel.Error
                        : context.Response.StatusCode >= StatusCodes.Status400BadRequest
                            ? LogLevel.Warning
                            : LogLevel.Information;

                    logger.Log(
                        logLevel,
                        "HTTP {HttpMethod} {RouteTemplate} returned {StatusCode} in {RequestDurationMilliseconds} ms",
                        context.Request.Method,
                        routeTemplate,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}