using BusTicketStore.Domain.Common;
using BusTicketStore.Domain.Enums;
using BusTicketStore.Domain.Exceptions;

namespace BusTicketStore.Domain.ValueObjects;

/// <summary>
/// One priced group of passengers sharing a fare tier. The line keeps the unit amount and
/// the count, so a quote can explain its total instead of only reporting it.
/// </summary>
public sealed record PriceLineItem
{
    public PriceLineItem(FareTier fareTier, int passengerCount, decimal unitAmount)
    {
        if (!Enum.IsDefined(fareTier))
        {
            throw new DomainException($"'{fareTier}' is not a valid fare tier.");
        }

        if (passengerCount <= 0)
        {
            throw new DomainException("A price line needs at least one passenger.");
        }

        if (unitAmount <= 0)
        {
            throw new DomainException("A unit amount must be positive.");
        }

        if (!MoneyRounding.IsTwoDecimals(unitAmount))
        {
            throw new DomainException("A unit amount is CRC with at most 2 decimals.");
        }

        FareTier = fareTier;
        PassengerCount = passengerCount;
        UnitAmount = unitAmount;
    }

    public FareTier FareTier { get; }

    public int PassengerCount { get; }

    public decimal UnitAmount { get; }

    /// <summary>
    /// Count times unit amount, rounded once at the domain scale so the subtotal is a sum
    /// of already-rounded amounts and cannot drift.
    /// </summary>
    public decimal LineTotal => MoneyRounding.Round(UnitAmount * PassengerCount);
}
