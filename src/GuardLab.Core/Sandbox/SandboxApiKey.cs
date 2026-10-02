namespace GuardLab.Core.Sandbox;

public sealed class SandboxApiKey
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public required string DisplayName { get; init; }
    public required string MaskedValue { get; init; }
}
