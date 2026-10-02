namespace GuardLab.Core.AdminAuth;

public sealed class AdminUser
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public string Role { get; init; } = "admin";
    public DateTimeOffset CreatedAt { get; init; }
}
