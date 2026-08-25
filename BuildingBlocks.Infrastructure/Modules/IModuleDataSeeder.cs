using BuildingBlocks.Core.Interfaces;

namespace BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// Extends a module with optional development-data seeding capabilities.
/// </summary>
public interface IModuleDataSeeder : IModule
{
    /// <summary>
    /// Seeds data owned by the module.
    /// </summary>
    Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
