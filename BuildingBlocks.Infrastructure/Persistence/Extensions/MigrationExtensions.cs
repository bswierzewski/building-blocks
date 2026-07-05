using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Persistence.Extensions;

/// <summary>
/// Provides helpers for applying Entity Framework Core migrations during application startup.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Checks for and applies any pending Entity Framework Core database migrations for the specified DbContext.
    /// </summary>
    public static async Task MigrateDatabaseAsync<TContext>(this IServiceProvider services, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
        var logger = services.GetRequiredService<ILogger<TContext>>();

        try
        {
            var contextName = typeof(TContext).Name;

            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            logger.LogInformation("Checking migrations for {ContextName}...", contextName);

            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pendingMigrations.Count > 0)
            {
                logger.LogInformation(
                    "Found {Count} pending migrations for {ContextName}: [{Migrations}]",
                    pendingMigrations.Count,
                    contextName,
                    string.Join(", ", pendingMigrations));

                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("{ContextName} database migrations applied successfully", contextName);
            }
            else
            {
                logger.LogInformation("No pending migrations for {ContextName} - database is up to date", contextName);
            }
        }
        catch (Exception exception)
        {
            var contextName = typeof(TContext).Name;
            logger.LogError(exception, "Error occurred during {ContextName} database migration", contextName);
            throw;
        }
    }
}
