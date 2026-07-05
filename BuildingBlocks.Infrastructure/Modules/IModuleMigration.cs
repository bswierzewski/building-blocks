using BuildingBlocks.Core.Interfaces;

namespace BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// Extends a module with explicit database or infrastructure migration hooks.
/// </summary>
public interface IModuleMigration : IModule
{
    /// <summary>
    /// Applies any pending module-owned migrations.
    /// </summary>
    Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
