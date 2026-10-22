using BusTicketStore.Domain.Enums;
using BusTicketStore.Domain.Exceptions;
using BusTicketStore.Domain.ValueObjects;

namespace BusTicketStore.Domain.Entities;

/// <summary>
/// A scheduled departure of a route on a date. The trip owns its presented stop order:
/// callers never reverse anything themselves. Direction is a presentation and itinerary
/// concern only and never changes the price.
/// </summary>
public sealed class Trip
{
    public Trip(
        Route route,
        Direction direction,
        DateOnly date,
        TimeOnly departureTime,
        ServiceClass serviceClass,
        int capacity)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (capacity <= 0)
        {
            throw new DomainException("Trip capacity must be positive.");
        }

        if (!Enum.IsDefined(direction))
        {
            throw new DomainException($"'{direction}' is not a valid travel direction.");
        }

        if (!Enum.IsDefined(serviceClass))
        {
            throw new DomainException($"'{serviceClass}' is not a valid service class.");
        }

        Route = route;
        Direction = direction;
        Date = date;
        DepartureTime = departureTime;
        ServiceClass = serviceClass;
        Capacity = capacity;

        var presentedStops = route.Stops.ToArray();

        if (direction == Direction.Reverse)
        {
            Array.Reverse(presentedStops);
        }

        Stops = Array.AsReadOnly(presentedStops);
    }

    public Route Route { get; }

    public Direction Direction { get; }

    public DateOnly Date { get; }

    public TimeOnly DepartureTime { get; }

    public ServiceClass ServiceClass { get; }

    public int Capacity { get; }

    /// <summary>
    /// The stops in the order this trip actually serves them: forward for a forward trip,
    /// reversed for a reverse trip. Computed once so no caller has to reason about
    /// direction again.
    /// </summary>
    public IReadOnlyList<Stop> Stops { get; }

    /// <summary>
    /// Boarding stop of this trip, derived from its own presented order.
    /// </summary>
    public Stop Origin => Stops[0];

    /// <summary>
    /// Alighting stop of this trip, derived from its own presented order.
    /// </summary>
    public Stop Destination => Stops[^1];
}
