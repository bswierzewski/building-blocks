using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.PublicEndpoints.Extensions;

/// <summary>
/// Protection of the endpoints marked with <see cref="PublicEndpointAttribute"/>: a limit of requests per IP address (set on
/// the attribute, 120 a minute unless it says otherwise) and headers that keep the answers out of caches and search engines.
/// The endpoint is found by its metadata, so the middleware has to run after routing (the implicit routing of
/// <c>WebApplication</c> is at the start of the pipeline).
/// </summary>
/// <remarks>
/// A request that matches no endpoint (a typo in the address) has no metadata and gets neither; it returns nothing worth
/// protecting. An endpoint that was matched and then answers 404 or fails is protected, because the answer is its own.
/// Behind a reverse proxy the limit counts the address of the proxy until <c>ForwardedHeaders</c> are configured.
/// See Docs/backend/multi-tenancy.md for how such an endpoint finds its tenant.
/// </remarks>
public static class PublicEndpointExtensions
{
    private static PublicEndpointAttribute? PublicOf(HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<PublicEndpointAttribute>();

    public static IServiceCollection AddPublicEndpointProtection(this IServiceCollection services)
        => services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                if (PublicOf(context) is not { } endpoint)
                    return RateLimitPartition.GetNoLimiter("not-public");

                // The endpoint is part of the key, so that each public endpoint counts the requests to it alone.
                var key = $"{context.GetEndpoint()!.DisplayName}|{context.Connection.RemoteIpAddress}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = endpoint.RequestsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        });

    /// <summary>
    /// Goes before the exception handler: the headers are added when the response starts, so they stay on error answers
    /// (a 404 included), which the exception handler would otherwise clear.
    /// </summary>
    public static IApplicationBuilder UsePublicEndpointProtection(this IApplicationBuilder app)
    {
        app.Use((context, next) =>
        {
            if (PublicOf(context) is not null)
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.CacheControl = "no-store";
                    context.Response.Headers["X-Robots-Tag"] = "noindex";

                    return Task.CompletedTask;
                });
            }

            return next();
        });

        return app.UseRateLimiter();
    }
}
