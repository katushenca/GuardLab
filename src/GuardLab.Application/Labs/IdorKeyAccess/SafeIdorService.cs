namespace GuardLab.Application.Labs.IdorKeyAccess;

using Abstractions;

public sealed class SafeIdorService(ISandboxKeyRepository repository)
{
    public async Task<IdorLookupResult> GetKeyAsync(Guid accountId, Guid keyId, CancellationToken cancellationToken = default)
    {
        var key = await repository.GetByIdAsync(keyId, cancellationToken);
        if (key is null) return new IdorLookupResult(404, null);
        if (key.AccountId != accountId) return new IdorLookupResult(403, new { error = "access_denied", trainingOnly = true });
        return new IdorLookupResult(200, new { key.Id, key.DisplayName, key.MaskedValue, ownerAccountId = key.AccountId, trainingOnly = true });
    }
}
