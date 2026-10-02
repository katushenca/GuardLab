namespace GuardLab.Core.Sandbox;

public sealed class SandboxAccount
{
    public Guid Id { get; init; }
    public required string Alias { get; init; }
}
