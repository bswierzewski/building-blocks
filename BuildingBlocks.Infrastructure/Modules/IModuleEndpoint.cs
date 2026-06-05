using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// Extends a module with explicit ASP.NET Core endpoint mapping.
/// </summary>
public interface IModuleEndpoint : IModule
{
    /// <summary>
    /// Maps the module's HTTP endpoints into the application's route builder.
    /// </summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}