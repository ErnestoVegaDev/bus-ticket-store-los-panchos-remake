namespace BusTicketStore.Domain.Enums;

/// <summary>
/// Travel direction of a trip over a route. The numeric values are fixed explicitly so
/// persisted trips keep their meaning even if the declaration order ever changes.
/// </summary>
public enum Direction
{
    Forward = 0,
    Reverse = 1
}
