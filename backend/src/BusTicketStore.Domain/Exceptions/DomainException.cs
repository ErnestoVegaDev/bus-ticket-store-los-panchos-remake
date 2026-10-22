namespace BusTicketStore.Domain.Exceptions;

/// <summary>
/// Raised when a domain rule is broken. Callers translate it into their own concern
/// (an HTTP problem response, a validation result) and never swallow it: a broken
/// invariant is a bug or an invalid request, never a condition to ignore.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
