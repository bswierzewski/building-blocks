using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Modules.Extensions;

/// <summary>
/// Provides endpoint mapping helpers for module-owned ASP.NET Core endpoints.
/// </summary>
public static class EndpointsExtensions
{
    /// <summary>
    /// Maps all module-owned ASP.NET Core endpoints.
    /// </summary>
    public static void MapModuleEndpoints(this WebApplication app)
    {
        var modules = app.Services.GetServices<IModuleEndpoint>();

        foreach (var module in modules)
            module.MapEndpoints(app);
    }
}