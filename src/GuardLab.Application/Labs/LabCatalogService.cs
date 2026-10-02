using GuardLab.Application.Abstractions;
using GuardLab.Core.Labs;

namespace GuardLab.Application.Labs;

public sealed class LabCatalogService(ILabRepository repository)
{
    public Task<IReadOnlyList<Lab>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
        repository.GetPublishedAsync(cancellationToken);

    public Task<Lab?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        repository.GetBySlugAsync(slug, cancellationToken);
}
