using BusTicketStore.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusTicketStore.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "BusTicketStore";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                $"Set 'ConnectionStrings:{ConnectionStringName}' in user secrets " +
                "or in an environment variable.");

        services.AddDbContext<BusTicketStoreContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
