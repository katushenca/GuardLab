namespace GuardLab.Application.Labs;

using GuardLab.Application.Labs.IdorKeyAccess;

public sealed class LabRunService(VulnerableIdorService vulnerable, SafeIdorService safe)
{
    public async Task<LabRunResult> RunAsync(Guid labId, string sid, Guid keyId, CancellationToken cancellationToken = default)
    {
        if (labId != IdorTrainingData.LabId) throw new InvalidOperationException("This lab does not expose a runnable scenario.");
        var accountId = IdorTrainingData.ResolveSession(sid)
            ?? throw new ArgumentException("The sid is not allowed for this training scenario.", nameof(sid));
        if (keyId != IdorTrainingData.AliceKeyId && keyId != IdorTrainingData.BobKeyId) throw new ArgumentException("The keyId is not allowed for this training scenario.", nameof(keyId));
        var vulnerableResult = await vulnerable.GetKeyAsync(keyId, cancellationToken);
        var safeResult = await safe.GetKeyAsync(accountId, keyId, cancellationToken);
        return new LabRunResult(vulnerableResult.StatusCode, safeResult.StatusCode, vulnerableResult.Response, safeResult.Response, $"Сессия {sid} принадлежит учебному аккаунту. Уязвимая версия ищет ключ только по идентификатору, защищённая сравнивает владельца ключа с аккаунтом сессии.");
    }
}
