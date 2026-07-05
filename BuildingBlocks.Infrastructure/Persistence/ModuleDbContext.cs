using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for modules that centralizes schema and configuration discovery.
/// </summary>
public abstract class ModuleDbContext<TContext>(DbContextOptions<TContext> options, string schema) : DbContext(options)
    where TContext : ModuleDbContext<TContext>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
