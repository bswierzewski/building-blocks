using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace BuildingBlocks.Infrastructure.Wolverine.Extensions;

/// <summary>
/// Provides Refit client registrations compatible with Wolverine's strict service-location policy.
/// </summary>
public static class RefitWolverineExtensions
{
    /// <summary>
    /// Registers a Refit client through <see cref="IHttpClientFactory"/> and explicitly allows
    /// Wolverine-generated code to resolve the opaque typed-client registration from DI.
    /// </summary>
    public static IHttpClientBuilder AddWolverineRefitClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services.AddRefitClient<TClient>();
        services.AllowWolverineServiceLocationFor<TClient>();

        return builder;
    }
}
