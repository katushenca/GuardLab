using GuardLab.Core.AdminAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GuardLab.Infrastructure.Persistence.Platform;

public sealed class AdminUserSeeder(
    PlatformDbContext db,
    IPasswordHasher<AdminUser> passwordHasher,
    IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var email = configuration["Admin:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Admin:Email and Admin:Password are required.");

        if (await db.AdminUsers.AnyAsync(x => x.Email == email, cancellationToken))
            return;

        var draft = new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = string.Empty,
            Role = "admin",
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.AdminUsers.Add(new AdminUser
        {
            Id = draft.Id,
            Email = draft.Email,
            PasswordHash = passwordHasher.HashPassword(draft, password),
            Role = draft.Role,
            CreatedAt = draft.CreatedAt
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
