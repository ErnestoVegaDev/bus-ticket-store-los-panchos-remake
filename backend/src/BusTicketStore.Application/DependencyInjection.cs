using BusTicketStore.Application.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BusTicketStore.Application;

/// <summary>
/// The composition entry point of the application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers everything this layer owns: the MediatR handlers, their FluentValidation
    /// validators, and the pipeline behaviors that wrap a handler call.
    /// </summary>
    /// <remarks>
    /// The Api project calls this and never enumerates handlers, validators or behaviors
    /// itself. Those lists change with every use case, and the composition root is not the
    /// place to keep them in sync.
    /// </remarks>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scans this assembly for IRequestHandler implementations. Handlers are internal
        // wiring, so they are never registered by hand.
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Order matters and is not decorative. The first behavior registered is the
            // outermost one, so validation runs before anything else can open a
            // transaction, touch the database or call an external service.
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation is scanned, not registered validator by validator: the moment a
        // validator class is added next to its command, this line picks it up.
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
