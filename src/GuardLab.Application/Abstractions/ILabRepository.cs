using GuardLab.Core.Labs;

namespace GuardLab.Application.Abstractions;

public interface ILabRepository
{
    Task<IReadOnlyList<Lab>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lab>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
    Task<Lab?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Lab?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Lab> AddAsync(Lab lab, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lab lab, CancellationToken cancellationToken = default);
}
