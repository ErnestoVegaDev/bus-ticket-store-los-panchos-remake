using BusTicketStore.Domain.Exceptions;
using BusTicketStore.Domain.ValueObjects;

namespace BusTicketStore.Domain.Entities;

/// <summary>
/// Route 644 as an ordered itinerary. The route owns its stop list and exposes it
/// read-only, so the endpoints and the fare table can only ever observe the same
/// sequence the route was built with.
/// </summary>
public sealed class Route
{
    public Route(
        string code,
        string name,
        decimal distanceKm,
        int durationMinutes,
        IEnumerable<Stop> stops)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (distanceKm <= 0)
        {
            throw new DomainException("Route distance must be positive.");
        }

        if (durationMinutes <= 0)
        {
            throw new DomainException("Route duration must be positive.");
        }

        ArgumentNullException.ThrowIfNull(stops);

        Code = code.Trim();
        Name = name.Trim();
        DistanceKm = distanceKm;
        DurationMinutes = durationMinutes;

        var orderedStops = stops.ToArray();

        if (orderedStops.Length < 2)
        {
            throw new DomainException("A route needs at least an origin and a destination.");
        }

        for (var index = 0; index < orderedStops.Length; index++)
        {
            if (orderedStops[index].Sequence != index + 1)
            {
                throw new DomainException("Route stops must be listed from sequence 1 with no gaps.");
            }
        }

        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var stop in orderedStops)
        {
            if (!seenNames.Add(stop.Name))
            {
                throw new DomainException($"The route contains the duplicate stop name '{stop.Name}'.");
            }
        }

        Stops = Array.AsReadOnly(orderedStops);
    }

    public string Code { get; }

    public string Name { get; }

    public decimal DistanceKm { get; }

    public int DurationMinutes { get; }

    public IReadOnlyList<Stop> Stops { get; }

    /// <summary>
    /// The first stop of the itinerary. Derived from <see cref="Stops"/> so the declared
    /// origin can never disagree with the stop list.
    /// </summary>
    public Stop Origin => Stops[0];

    /// <summary>
    /// The last stop of the itinerary. Derived from <see cref="Stops"/> so the declared
    /// destination can never disagree with the stop list.
    /// </summary>
    public Stop Destination => Stops[^1];
}
