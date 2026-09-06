using BuildingBlocks.Clerk.Client;
using BuildingBlocks.Clerk.Client.Handlers;
using BuildingBlocks.Clerk.Options;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Wolverine.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Clerk;

/// <summary>
/// Registers the Clerk Backend API client.
/// </summary>
public sealed class ClerkModule : IModule
{
    public string Name => "Clerk";

    /// <summary>
    /// Registers the Refit-based Clerk API client and supporting services.
    /// </summary>
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ClerkOptions>()
            .Bind(configuration.GetSection(ClerkOptions.SectionName));

        services.AddTransient<ClerkAuthenticationHandler>();

        services.AddWolverineRefitClient<IClerkHttpClient>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<ClerkOptions>>().Value;
                client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<ClerkAuthenticationHandler>();
    }
}
