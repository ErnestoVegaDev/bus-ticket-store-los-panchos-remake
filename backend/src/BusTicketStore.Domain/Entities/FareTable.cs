using BusTicketStore.Domain.Enums;
using BusTicketStore.Domain.Exceptions;
using BusTicketStore.Domain.ValueObjects;

namespace BusTicketStore.Domain.Entities;

/// <summary>
/// The fare matrix of one route. The lookup key is the pair (service class, passenger
/// tier) and deliberately holds no direction: route 644 is the same product in both
/// directions, so symmetry is structural rather than something a caller must remember.
/// </summary>
public sealed class FareTable
{
    private const decimal DirectoEstandarAmount = 825.00m;
    private const decimal DirectoCiudadanoDeOroAmount = 415.00m;
    private const decimal ColectivoEstandarAmount = 550.00m;
    private const decimal ColectivoCiudadanoDeOroAmount = 275.00m;

    private readonly Dictionary<(ServiceClass ServiceClass, FareTier FareTier), Fare> _faresByKey;

    public FareTable(Route route, IEnumerable<Fare> fares)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(fares);

        Route = route;
        _faresByKey = new Dictionary<(ServiceClass ServiceClass, FareTier FareTier), Fare>();

        foreach (var fare in fares)
        {
            if (!_faresByKey.TryAdd((fare.ServiceClass, fare.FareTier), fare))
            {
                throw new DomainException(
                    $"A fare for service class '{fare.ServiceClass}' and tier '{fare.FareTier}' is already configured.");
            }
        }

        if (_faresByKey.Count == 0)
        {
            throw new DomainException("A fare table needs at least one fare.");
        }

        Fares = Array.AsReadOnly(_faresByKey.Values.ToArray());
    }

    public Route Route { get; }

    public IReadOnlyCollection<Fare> Fares { get; }

    /// <summary>
    /// Returns the configured fare for the pair, or fails. It never returns null or zero:
    /// an unpriced combination must stop the sale instead of silently undercharging.
    /// </summary>
    public Fare Resolve(ServiceClass serviceClass, FareTier fareTier)
    {
        if (_faresByKey.TryGetValue((serviceClass, fareTier), out var fare))
        {
            return fare;
        }

        throw new DomainException(
            $"No fare is configured for service class '{serviceClass}' and tier '{fareTier}'.");
    }

    /// <summary>
    /// The fare matrix of route 644, defined once as domain data. Route 644 has a single
    /// table for both directions, which is why the amounts are keyed only by class and
    /// tier.
    /// </summary>
    public static FareTable ForRoute644(Route route)
    {
        ArgumentNullException.ThrowIfNull(route);

        return new FareTable(
            route,
            new[]
            {
                new Fare(ServiceClass.Directo, FareTier.Estandar, DirectoEstandarAmount),
                new Fare(ServiceClass.Directo, FareTier.CiudadanoDeOro, DirectoCiudadanoDeOroAmount),
                new Fare(ServiceClass.Colectivo, FareTier.Estandar, ColectivoEstandarAmount),
                new Fare(ServiceClass.Colectivo, FareTier.CiudadanoDeOro, ColectivoCiudadanoDeOroAmount)
            });
    }
}
