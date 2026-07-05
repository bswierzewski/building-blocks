using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BuildingBlocks.Infrastructure.Persistence
{
    /// <summary>
    /// Creates module DbContext instances for EF Core design-time tooling without bootstrapping the full application.
    /// </summary>
    public abstract class ModuleDbContextDesignTimeFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        private const string DesignTimeConnectionString = "Host=_design-time_;Database=_design-time_";

        /// <summary>
        /// Builds a DbContext instance backed by the PostgreSQL provider for migration scaffolding.
        /// </summary>
        public TContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseNpgsql(DesignTimeConnectionString);

            if (Activator.CreateInstance(typeof(TContext), optionsBuilder.Options) is not TContext dbContext)
                throw new InvalidOperationException($"Could not create {typeof(TContext).Name} using the expected DbContextOptions constructor.");

            return dbContext;
        }
    }
}
