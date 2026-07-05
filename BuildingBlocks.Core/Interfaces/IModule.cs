using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Contract for application modules that can register services and expose runtime metadata.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Logical module name used for diagnostics and configuration grouping.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Registers module services in the DI container.
    /// </summary>
    void AddServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Runs post-build module setup against the fully constructed application's service provider.
    /// </summary>
    Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
