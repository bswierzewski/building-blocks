using BuildingBlocks.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Modules.Extensions;

/// <summary>
/// Provides service-provider-based helpers for module startup hooks and module migrations.
/// </summary>
public static class ModuleLifecycleExtensions
{
    /// <summary>
    /// Runs all module initialization hooks registered in the current container.
    /// </summary>
    public static async Task InitializeModulesAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        foreach (var module in services.GetServices<IModule>())
            await module.InitializeAsync(services, cancellationToken);
    }

    /// <summary>
    /// Applies all module-owned migrations registered in the current container.
    /// </summary>
    public static async Task ApplyModuleMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        foreach (var migration in services.GetServices<IModuleMigration>())
            await migration.MigrateAsync(services, cancellationToken);
    }

    /// <summary>
    /// Runs development-data seeders exposed by registered modules.
    /// </summary>
    public static async Task ApplyModuleDataSeedingAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        foreach (var dataSeeder in services.GetServices<IModuleDataSeeder>())
            await dataSeeder.SeedAsync(services, cancellationToken);
    }
}
