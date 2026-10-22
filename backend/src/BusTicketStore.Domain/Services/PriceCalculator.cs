using BusTicketStore.Domain.Constants;
using BusTicketStore.Domain.Entities;
using BusTicketStore.Domain.Enums;
using BusTicketStore.Domain.Exceptions;
using BusTicketStore.Domain.ValueObjects;

namespace BusTicketStore.Domain.Services;

/// <summary>
/// Prices a trip for a set of passengers. The discount is per passenger, not per order:
/// every passenger tier is resolved independently against the fare table.
/// </summary>
public sealed class PriceCalculator
{
    private readonly FareTable _fareTable;

    public PriceCalculator(FareTable fareTable)
    {
        ArgumentNullException.ThrowIfNull(fareTable);

        _fareTable = fareTable;
    }

    /// <summary>
    /// Builds a quote for the given passenger tiers. Each distinct tier becomes one line
    /// so the passenger sees the standard and discounted portions separately.
    /// </summary>
    public PriceQuote Quote(Trip trip, IReadOnlyList<FareTier> passengerTiers)
    {
        ArgumentNullException.ThrowIfNull(trip);
        ArgumentNullException.ThrowIfNull(passengerTiers);

        if (passengerTiers.Count == 0)
        {
            throw new DomainException("A quote needs at least one passenger.");
        }

        foreach (var tier in passengerTiers)
        {
            if (!Enum.IsDefined(tier))
            {
                throw new DomainException($"'{tier}' is not a valid fare tier.");
            }
        }

        if (!string.Equals(_fareTable.Route.Code, trip.Route.Code, StringComparison.Ordinal))
        {
            throw new DomainException(
                $"A fare table for route '{_fareTable.Route.Code}' cannot price a trip on route '{trip.Route.Code}'.");
        }

        // trip.Direction is intentionally never read here. Reading it would break the
        // symmetry rule: a trip costs the same in both directions.
        var lineItems = new List<PriceLineItem>();

        foreach (var group in passengerTiers.GroupBy(tier => tier).OrderBy(group => (int)group.Key))
        {
            var fare = _fareTable.Resolve(trip.ServiceClass, group.Key);
            lineItems.Add(new PriceLineItem(group.Key, group.Count(), fare.Amount));
        }

        return new PriceQuote(lineItems, TaxPolicy.IvaRate);
    }
}
