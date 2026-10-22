using BusTicketStore.Application.Exceptions;
using FluentValidation;
using MediatR;

namespace BusTicketStore.Application.Behaviours;

/// <summary>
/// Runs every FluentValidation validator registered for the incoming request before the
/// handler executes.
/// </summary>
/// <remarks>
/// This is a MediatR pipeline behavior and not a Minimal API endpoint filter on purpose.
/// Validation belongs to the use case, not to the transport, so a command dispatched from
/// an HTTP endpoint, a background job or a test is validated in exactly the same way and
/// exactly once. An endpoint filter would validate only the HTTP path, and would force the
/// same rules to be repeated for every entry point.
///
/// Ordering is established in <c>AddApplication</c>: this behavior is registered first so
/// a malformed request never reaches a handler that could open a transaction, hit the
/// database or call the payment gateway.
///
/// <c>BusTicketStore.Application.Exceptions.ValidationException</c> is aliased because
/// FluentValidation exposes a type with the same name in the namespace imported above, and
/// the two are not interchangeable: this behavior throws the application one.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Requests that declare no validator pay only this check. The collection is
        // injected empty instead of failing, because "not validated yet" is a legitimate
        // state while the use case surface is still growing.
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        // Validators are async-capable, so they run concurrently: a rule that reads from
        // the database does not serialise the rules that only inspect the request.
        var results = await Task.WhenAll(
            validators.Select(async validator => await validator.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length > 0)
        {
            var errors = failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            throw new Exceptions.ValidationException(errors);
        }

        return await next();
    }
}
