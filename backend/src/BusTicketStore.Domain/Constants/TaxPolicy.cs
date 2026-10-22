namespace BusTicketStore.Domain.Constants;

/// <summary>
/// Tax rules of the fare. The IVA rate is a domain constant, never configuration: a
/// deployment must not be able to change what a ticket legally costs.
/// </summary>
public static class TaxPolicy
{
    /// <summary>
    /// Costa Rican IVA applied to the ticket subtotal.
    /// </summary>
    public const decimal IvaRate = 0.13m;
}
