using BuildingBlocks.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Modules.Extensions;

/// <summary>
/// Provides registration helpers for application modules and their infrastructure capabilities.
/// </summary>
public static class ModuleRegistrationExtensions
{
    /// <summary>
    /// Registers the supplied modules together with their endpoint, migration and data-seeding contracts.
    /// </summary>
    public static IServiceCollection RegisterModules(
        this IServiceCollection services,
        IConfiguration configuration,
        IEnumerable<IModule> modules)
    {
        foreach (var module in modules)
        {
            services.AddSingleton(module);

            if (module is IModuleEndpoint endpointModule)
                services.AddSingleton(endpointModule);

            if (module is IModuleMigration migrationModule)
                services.AddSingleton(migrationModule);

            if (module is IModuleDataSeeder dataSeederModule)
                services.AddSingleton(dataSeederModule);

            module.AddServices(services, configuration);
        }

        return services;
    }
}
