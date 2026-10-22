namespace BusTicketStore.Application.Interfaces;

/// <summary>
/// Commits every change tracked during a single use case as one atomic unit.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists the work accumulated by the repositories used in the current scope.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
