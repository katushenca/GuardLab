using GuardLab.Application.Abstractions;
using GuardLab.Core.AdminAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GuardLab.Application.AdminAuth;

public sealed class AdminAuthService(IAdminUserRepository repository, IPasswordHasher<AdminUser> passwordHasher, IConfiguration configuration)
{
    public async Task<AdminLoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
            return null;

        var key = configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is missing.");
        if (Encoding.UTF8.GetByteCount(key) < 32) throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes.");
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "GuardLab",
            audience: configuration["Jwt:Audience"] ?? "GuardLab.Api",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim("role", user.Role)],
            expires: expiresAt,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        return new AdminLoginResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

public sealed record AdminLoginResult(string AccessToken, DateTime ExpiresAt);
    
