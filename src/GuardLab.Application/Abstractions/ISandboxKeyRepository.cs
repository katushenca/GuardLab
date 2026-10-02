using GuardLab.Core.Sandbox;

namespace GuardLab.Application.Abstractions;

public interface ISandboxKeyRepository
{
    Task<SandboxApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
