using GuardLab.Core.AdminAuth;

namespace GuardLab.Application.Abstractions;

public interface IAdminUserRepository
{
    Task<AdminUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
}
