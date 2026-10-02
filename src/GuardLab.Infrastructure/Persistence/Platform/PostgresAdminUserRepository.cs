using GuardLab.Application.Abstractions;
using GuardLab.Core.AdminAuth;
using Microsoft.EntityFrameworkCore;

namespace GuardLab.Infrastructure.Persistence.Platform;

public sealed class PostgresAdminUserRepository(PlatformDbContext db) : IAdminUserRepository
{
    public Task<AdminUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.AdminUsers.AsNoTracking().SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
}
