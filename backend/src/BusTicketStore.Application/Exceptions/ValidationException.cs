namespace BusTicketStore.Application.Exceptions;

/// <summary>
/// Raised by the validation behavior when a request fails its FluentValidation rules.
/// </summary>
/// <remarks>
/// It is deliberately separate from <c>DomainException</c>. A domain exception means the
/// request reached the domain and contradicted an invariant, so the caller disagreed with
/// the state of the system. A validation exception means the request never reached the
/// domain at all. Both are mapped to 400 by the HTTP layer, but the distinction stays
/// visible in logs and tests.
///
/// The failures are carried as a plain string dictionary, not as FluentValidation's
/// <c>ValidationFailure</c> list, so the Api layer can project them into a problem
/// response without referencing FluentValidation. The dependency graph stays one-way:
/// Api knows Application, and Application knows FluentValidation.
/// </remarks>
public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    /// Property name to the messages reported for it. One entry per field, not one entry
    /// per failed rule, so a client can render the errors next to the input they belong to.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
