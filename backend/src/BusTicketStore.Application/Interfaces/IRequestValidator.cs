using FluentValidation;

namespace BusTicketStore.Application.Interfaces;

/// <summary>
/// Marks a FluentValidation validator that validates one request of the application
/// layer. Application defines this alias for two reasons: a use case can declare its own
/// validator without spelling out the FluentValidation generic in its signature, and the
/// pipeline behavior can resolve validators through a type this assembly owns instead of
/// a third-party type.
/// </summary>
/// <remarks>
/// Implementing this interface is optional. A validator that implements
/// <see cref="IValidator{T}" /> directly is still discovered by the assembly scan, because
/// this interface inherits it.
/// </remarks>
public interface IRequestValidator<in TRequest> : IValidator<TRequest>
{
}
