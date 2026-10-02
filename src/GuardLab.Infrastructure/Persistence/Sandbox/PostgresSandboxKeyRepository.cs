using GuardLab.Application.Abstractions;
using GuardLab.Core.Sandbox;
using Microsoft.EntityFrameworkCore;

namespace GuardLab.Infrastructure.Persistence.Sandbox;

public sealed class PostgresSandboxKeyRepository(SandboxDbContext db) : ISandboxKeyRepository
{
    public Task<SandboxApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.ApiKeys.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<SandboxAccount?> GetAccountAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
