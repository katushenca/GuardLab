using GuardLab.Application.AdminAuth;
using Microsoft.AspNetCore.Mvc;

namespace GuardLab.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AdminAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result is null)
        {
            logger.LogWarning(
                "Application event {EventName} outcome {Outcome}",
                "auth.login",
                "failure");
            return Unauthorized(new { error = "invalid_credentials" });
        }

        logger.LogInformation(
            "Application event {EventName} outcome {Outcome}",
            "auth.login",
            "success");
        return Ok(result);
    }
}

public sealed record AdminLoginRequest(string Email, string Password);