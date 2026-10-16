using GuardLab.Application.Labs.IdorKeyAccess;
using Microsoft.Extensions.Logging;

namespace GuardLab.Application.Labs;

public sealed class LabRunService(
    VulnerableIdorService vulnerable,
    SafeIdorService safe,
    ILogger<LabRunService> logger)
{
    public async Task<LabRunResult> RunAsync(Guid labId, string sid, Guid keyId,
        CancellationToken cancellationToken = default)
    {
        if (labId != IdorTrainingData.LabId)
        {
            LogRejectedRun(labId, "unsupported_lab");
            throw new InvalidOperationException("This lab does not expose a runnable scenario.");
        }

        var accountId = IdorTrainingData.ResolveSession(sid);
        if (accountId is null)
        {
            LogRejectedRun(labId, "invalid_training_session");
            throw new ArgumentException("The sid is not allowed for this training scenario.", nameof(sid));
        }

        if (keyId != IdorTrainingData.AliceKeyId && keyId != IdorTrainingData.BobKeyId)
        {
            LogRejectedRun(labId, "invalid_training_key");
            throw new ArgumentException("The keyId is not allowed for this training scenario.", nameof(keyId));
        }

        var vulnerableResult = await vulnerable.GetKeyAsync(keyId, cancellationToken);
        var safeResult = await safe.GetKeyAsync(accountId.Value, keyId, cancellationToken);

        logger.LogInformation(
            "Application event {EventName} outcome {Outcome} labId {LabId} vulnerableStatusCode {VulnerableStatusCode} safeStatusCode {SafeStatusCode}",
            "lab.run",
            "completed",
            labId,
            vulnerableResult.StatusCode,
            safeResult.StatusCode);

        return new LabRunResult(vulnerableResult.StatusCode, safeResult.StatusCode, vulnerableResult.Response,
            safeResult.Response,
            $"Сессия {sid} принадлежит учебному аккаунту. Уязвимая версия ищет ключ только по идентификатору, защищённая сравнивает владельца ключа с аккаунтом сессии.");
    }

    private void LogRejectedRun(Guid labId, string reason) => logger.LogWarning(
        "Application event {EventName} outcome {Outcome} reason {Reason} labId {LabId}",
        "lab.run",
        "rejected",
        reason,
        labId);
}