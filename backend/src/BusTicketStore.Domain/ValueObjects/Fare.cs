using BusTicketStore.Domain.Common;
using BusTicketStore.Domain.Enums;
using BusTicketStore.Domain.Exceptions;

namespace BusTicketStore.Domain.ValueObjects;

/// <summary>
/// The price of one seat for one service class and one passenger tier. A value object:
/// equal tier, class and amount describe the same fare, so fare lines can be compared
/// without identity.
/// </summary>
public sealed record Fare
{
    public Fare(ServiceClass serviceClass, FareTier fareTier, decimal amount)
    {
        if (!Enum.IsDefined(serviceClass))
        {
            throw new DomainException($"'{serviceClass}' is not a valid service class.");
        }

        if (!Enum.IsDefined(fareTier))
        {
            throw new DomainException($"'{fareTier}' is not a valid fare tier.");
        }

        if (amount <= 0)
        {
            throw new DomainException("A fare amount must be positive.");
        }

        if (!MoneyRounding.IsTwoDecimals(amount))
        {
            throw new DomainException("Fare amounts are CRC with at most 2 decimals.");
        }

        ServiceClass = serviceClass;
        FareTier = fareTier;
        Amount = amount;
    }

    public ServiceClass ServiceClass { get; }

    public FareTier FareTier { get; }

    public decimal Amount { get; }
}
