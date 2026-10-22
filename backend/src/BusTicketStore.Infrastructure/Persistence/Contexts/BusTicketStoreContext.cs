using Microsoft.EntityFrameworkCore;

namespace BusTicketStore.Infrastructure.Persistence.Contexts;

public sealed class BusTicketStoreContext(DbContextOptions<BusTicketStoreContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusTicketStoreContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
