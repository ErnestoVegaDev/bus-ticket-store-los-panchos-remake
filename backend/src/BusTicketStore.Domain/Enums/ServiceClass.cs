namespace BusTicketStore.Domain.Enums;

/// <summary>
/// Service level sold for a trip. The class is part of the commercial product, so it is
/// modelled in the domain and stored with explicit numeric values.
/// </summary>
public enum ServiceClass
{
    Directo = 0,
    Colectivo = 1
}
