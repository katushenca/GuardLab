using GuardLab.Application.Health;
using Microsoft.AspNetCore.Mvc;

namespace GuardLab.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(HealthService healthService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await healthService.CheckAsync(cancellationToken);
        var body = new
        {
            status = result.IsHealthy ? "ok" : "degraded",
            database = result.IsHealthy ? "ok" : "unavailable"
        };

        return result.IsHealthy ? Ok(body) : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}
