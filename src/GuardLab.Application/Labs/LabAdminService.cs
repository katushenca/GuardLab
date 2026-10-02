using GuardLab.Application.Abstractions;
using GuardLab.Core.Labs;

namespace GuardLab.Application.Labs;

public sealed class LabAdminService(ILabRepository repository)
{
    public Task<IReadOnlyList<Lab>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<Lab> CreateAsync(Lab lab, CancellationToken cancellationToken = default)
    {
        if (await repository.SlugExistsAsync(lab.Slug, cancellationToken: cancellationToken))
            throw new DuplicateLabSlugException(lab.Slug);
        return await repository.AddAsync(lab, cancellationToken);
    }

    public async Task UpdateAsync(Lab lab, CancellationToken cancellationToken = default)
    {
        if (await repository.SlugExistsAsync(lab.Slug, lab.Id, cancellationToken))
            throw new DuplicateLabSlugException(lab.Slug);
        await repository.UpdateAsync(lab, cancellationToken);
    }

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lab = await repository.GetByIdAsync(id, cancellationToken);
        if (lab is null) return false;
        lab.TransitionTo(LabStatus.Archived);
        await repository.UpdateAsync(lab, cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lab = await repository.GetByIdAsync(id, cancellationToken);
        if (lab is null) return false;
        lab.TransitionTo(LabStatus.Draft);
        await repository.UpdateAsync(lab, cancellationToken);
        return true;
    }

    public Task<Lab?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.GetByIdAsync(id, cancellationToken);
}

public sealed class DuplicateLabSlugException(string slug) : Exception($"A lab with slug '{slug}' already exists.");
