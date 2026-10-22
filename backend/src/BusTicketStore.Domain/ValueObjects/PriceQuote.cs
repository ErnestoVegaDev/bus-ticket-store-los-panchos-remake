using BusTicketStore.Domain.Common;
using BusTicketStore.Domain.Exceptions;

namespace BusTicketStore.Domain.ValueObjects;

/// <summary>
/// The priced result of a purchase. The quote computes its own totals from its lines, so
/// no caller can submit a total that disagrees with the detail it is paying for.
/// </summary>
public sealed class PriceQuote
{
    public PriceQuote(IEnumerable<PriceLineItem> lineItems, decimal ivaRate)
    {
        ArgumentNullException.ThrowIfNull(lineItems);

        var items = lineItems.ToArray();

        if (items.Length == 0)
        {
            throw new DomainException("A quote needs at least one line item.");
        }

        if (ivaRate <= 0m || ivaRate >= 1m)
        {
            throw new DomainException("The IVA rate must be greater than 0 and less than 1.");
        }

        LineItems = Array.AsReadOnly(items);
        PassengerCount = items.Sum(item => item.PassengerCount);
        Subtotal = MoneyRounding.Round(items.Sum(item => item.LineTotal));
        Iva = MoneyRounding.Round(Subtotal * ivaRate);
        Total = MoneyRounding.Round(Subtotal + Iva);
    }

    public IReadOnlyList<PriceLineItem> LineItems { get; }

    public int PassengerCount { get; }

    public decimal Subtotal { get; }

    /// <summary>
    /// IVA applied once on the subtotal, never per passenger. Computing it per passenger
    /// and rounding each time would produce a different, larger total.
    /// </summary>
    public decimal Iva { get; }

    public decimal Total { get; }
}
