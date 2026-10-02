namespace GuardLab.Application.Labs;

public sealed record LabRunResult(
    int VulnerableStatusCode,
    int SafeStatusCode,
    object? VulnerableResponse,
    object? SafeResponse,
    string Explanation);
