namespace GuardLab.Application.Health;

public interface IDatabaseHealthProbe
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}
