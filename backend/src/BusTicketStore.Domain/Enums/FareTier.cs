namespace BusTicketStore.Domain.Enums;

/// <summary>
/// Passenger fare tier. The tier decides the amount a passenger pays, so it is part of
/// the domain language rather than a presentation flag. Numeric values are explicit for
/// persistence safety.
/// </summary>
public enum FareTier
{
    Estandar = 0,
    CiudadanoDeOro = 1
}
