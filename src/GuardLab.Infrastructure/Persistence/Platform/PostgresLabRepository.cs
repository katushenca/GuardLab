using GuardLab.Application.Abstractions;
using GuardLab.Core.Labs;
using Microsoft.EntityFrameworkCore;

namespace GuardLab.Infrastructure.Persistence.Platform;

public sealed class PostgresLabRepository(PlatformDbContext db) : ILabRepository
{
    public async Task<IReadOnlyList<Lab>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
        await db.Labs.AsNoTracking().Where(x => x.Status == LabStatus.Published).OrderBy(x => x.Title).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Lab>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Labs.AsNoTracking().OrderBy(x => x.Status).ThenBy(x => x.Title).ToListAsync(cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) =>
        db.Labs.AnyAsync(x => x.Slug == slug && (!exceptId.HasValue || x.Id != exceptId.Value), cancellationToken);

    public Task<Lab?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        db.Labs.AsNoTracking().SingleOrDefaultAsync(x => x.Slug == slug && x.Status == LabStatus.Published, cancellationToken);

    public Task<Lab?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Labs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Lab> AddAsync(Lab lab, CancellationToken cancellationToken = default)
    {
        db.Labs.Add(lab);
        await db.SaveChangesAsync(cancellationToken);
        return lab;
    }

    public async Task UpdateAsync(Lab lab, CancellationToken cancellationToken = default)
    {
        db.Labs.Update(lab);
        await db.SaveChangesAsync(cancellationToken);
    }

}
