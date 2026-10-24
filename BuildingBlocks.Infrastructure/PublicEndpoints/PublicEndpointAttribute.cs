using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Infrastructure.PublicEndpoints;

/// <summary>
/// Marks an endpoint that is open to everyone, with no user. It is an <c>[AllowAnonymous]</c> that also switches on what a
/// public endpoint needs (see <c>PublicEndpointExtensions</c>): a limit of requests per IP address and headers that keep the
/// answers out of caches and search engines. Use it instead of <c>[AllowAnonymous]</c>, never together with it.
/// </summary>
/// <remarks>
/// The protection follows the attribute, not the address, so an endpoint cannot be public by accident of its path. A plain
/// <c>[AllowAnonymous]</c> is not protected, which is why a test lists every anonymous endpoint and fails on one it does not know.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PublicEndpointAttribute : AllowAnonymousAttribute
{
    public const int DefaultRequestsPerMinute = 120;

    /// <summary>
    /// How many requests one IP address can make to this endpoint in a minute; every public endpoint has its own count.
    /// </summary>
    public int RequestsPerMinute { get; init; } = DefaultRequestsPerMinute;
}
