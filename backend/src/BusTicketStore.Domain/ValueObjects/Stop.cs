using BusTicketStore.Domain.Exceptions;

namespace BusTicketStore.Domain.ValueObjects;

/// <summary>
/// A named boarding point on a route. A value object: two stops are the same stop when
/// their name, municipality and sequence match, so the itinerary can be compared and
/// copied safely.
/// </summary>
public sealed record Stop
{
    public Stop(string name, string municipality, int sequence)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(municipality);

        if (sequence < 1)
        {
            throw new DomainException("A stop sequence starts at 1.");
        }

        Name = name.Trim();
        Municipality = municipality.Trim();
        Sequence = sequence;
    }

    public string Name { get; }

    public string Municipality { get; }

    /// <summary>
    /// One-based position along the route. Starting at one lets the route assert a
    /// gapless order with a single comparison per stop.
    /// </summary>
    public int Sequence { get; }
}
