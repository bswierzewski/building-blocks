using System.ServiceModel;
using BuildingBlocks.Soap.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Soap.Middleware;

/// <summary>
/// Caching SOAP middleware that caches operation results.
/// Cache is per-client type with configurable TTL.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public sealed class CachingSoapMiddleware<TClient>(
    IMemoryCache memoryCache,
    ILogger<CachingSoapMiddleware<TClient>> logger,
    TimeSpan cacheDuration)
    : SoapMiddleware<TClient> where TClient : ICommunicationObject
{
    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = cacheDuration,
        Priority = CacheItemPriority.High,
        Size = 1
    };

    /// <summary>
    /// Attempts to serve the result from cache before invoking the next pipeline step.
    /// Caching is enabled only when the call context provides a cache key.
    /// </summary>
    public override async Task<TResult> InvokeAsync<TResult>(
        SoapCallContext context,
        Func<CancellationToken, Task<TResult>> next,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(context.CacheKey))
            return await next(ct);

        var cacheKey = $"soap_cache:{typeof(TClient).Name}:{context.CacheKey}";

        if (memoryCache.TryGetValue<TResult>(cacheKey, out var cachedResult))
        {
            logger.LogDebug("SOAP {ClientName} {OperationName} - cache hit", typeof(TClient).Name, context.OperationName);
            return cachedResult!;
        }

        logger.LogDebug("SOAP {ClientName} {OperationName} - cache miss, invoking", typeof(TClient).Name, context.OperationName);
        var result = await next(ct);

        memoryCache.Set(cacheKey, result, _cacheOptions);
        logger.LogInformation("SOAP {ClientName} {OperationName} - result cached for {Duration}",
            typeof(TClient).Name, context.OperationName, cacheDuration);

        return result;
    }

    /// <summary>
    /// Registers caching middleware for the specified SOAP client type.
    /// </summary>
    public static IServiceCollection Add(IServiceCollection services, TimeSpan cacheDuration)
    {
        services.AddScoped<SoapMiddleware<TClient>>(sp =>
            new CachingSoapMiddleware<TClient>(
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<ILogger<CachingSoapMiddleware<TClient>>>(),
                cacheDuration));

        return services;
    }
}
