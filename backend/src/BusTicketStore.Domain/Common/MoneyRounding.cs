namespace BusTicketStore.Domain.Common;

/// <summary>
/// The single rounding policy for every monetary value in the system. Centralising it is
/// what keeps a fare, a line total, a subtotal, the IVA and the grand total from drifting
/// apart by a cent depending on where the rounding happened.
/// </summary>
public static class MoneyRounding
{
    /// <summary>
    /// Number of decimals used by CRC amounts in this domain.
    /// </summary>
    public const int Scale = 2;

    /// <summary>
    /// Rounds a monetary amount using the domain's one and only policy. No other type may
    /// round money: a second policy would make the quote stop adding up.
    /// </summary>
    public static decimal Round(decimal amount) =>
        decimal.Round(amount, Scale, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Tells whether an amount is already representable at the domain scale, which is the
    /// precondition for accepting it as a fare or a unit price.
    /// </summary>
    public static bool IsTwoDecimals(decimal amount) => Round(amount) == amount;
}
