namespace GuardLab.Application.Labs.IdorKeyAccess;

public sealed record IdorLookupResult(int StatusCode, object? Response);

public static class IdorTrainingData
{
    public static readonly Guid AliceAccountId = Guid.Parse("20000000-0000-4000-8000-000000000001");
    public static readonly Guid AliceKeyId = Guid.Parse("30000000-0000-4000-8000-000000000001");
    public static readonly Guid BobKeyId = Guid.Parse("30000000-0000-4000-8000-000000000002");
    public static readonly Guid LabId = Guid.Parse("10000000-0000-4000-8000-000000000001");

    public static Guid? ResolveSession(string sid) => sid switch
    {
        "sid_alice_demo" => AliceAccountId,
        "sid_bob_demo" => Guid.Parse("20000000-0000-4000-8000-000000000002"),
        _ => null
    };
}
