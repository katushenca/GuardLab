using GuardLab.Application.AdminAuth;
using Microsoft.AspNetCore.Mvc;

namespace GuardLab.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AdminAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        return result is null
            ? Unauthorized(new { error = "invalid_credentials" })
            : Ok(result);
    }
}

public sealed record AdminLoginRequest(string Email, string Password);
