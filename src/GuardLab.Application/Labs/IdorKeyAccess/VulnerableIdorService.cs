namespace GuardLab.Application.Labs.IdorKeyAccess;

using Abstractions;

public sealed class VulnerableIdorService(ISandboxKeyRepository repository)
{
    public async Task<IdorLookupResult> GetKeyAsync(Guid keyId, CancellationToken cancellationToken = default)
    {
        var key = await repository.GetByIdAsync(keyId, cancellationToken);
        return key is null ? new IdorLookupResult(404, null) : new IdorLookupResult(200, new { key.Id, key.DisplayName, key.MaskedValue, ownerAccountId = key.AccountId, trainingOnly = true });
    }
}
