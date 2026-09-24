using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for modules that centralizes interceptors, schema, and configuration discovery.
/// </summary>
public abstract class ModuleDbContext<TContext>(DbContextOptions<TContext> options, string schema) : DbContext(options)
    where TContext : ModuleDbContext<TContext>
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Wolverine requires a constructor with only DbContextOptions, so resolve DI-owned interceptors here.
        var services = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider
            ?? throw new InvalidOperationException($"{GetType().Name} was created without an application service provider.");

        optionsBuilder.AddInterceptors([
            services.GetRequiredService<AuditableEntityInterceptor>()
        ]);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
