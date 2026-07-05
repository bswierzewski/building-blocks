using System.ServiceModel;
using BuildingBlocks.Soap.Abstractions;
using BuildingBlocks.Soap.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Soap.Builders;

/// <summary>
/// Extension methods for registering SOAP pipelines on the service collection.
/// </summary>
public static class SoapServiceCollectionExtensions
{
    /// <summary>
    /// Registers a SOAP pipeline for the specified client type and configures optional middleware.
    /// </summary>
    public static IServiceCollection AddSoap<TClient>(
        this IServiceCollection services,
        Func<TClient> clientFactory,
        Action<SoapPipelineBuilder<TClient>>? configure = null)
        where TClient : ICommunicationObject
    {
        services.AddSingleton(clientFactory);
        services.AddScoped<SoapPipeline<TClient>>();

        var builder = new SoapPipelineBuilder<TClient>(services);
        configure?.Invoke(builder);

        return services;
    }
}

/// <summary>
/// Fluent registration builder for SOAP middleware.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public sealed class SoapPipelineBuilder<TClient>(IServiceCollection services)
    where TClient : ICommunicationObject
{
    /// <summary>
    /// Adds logging middleware to the pipeline.
    /// </summary>
    public SoapPipelineBuilder<TClient> AddLogging()
    {
        LoggingSoapMiddleware<TClient>.Add(services);
        return this;
    }

    /// <summary>
    /// Adds resilience middleware to the pipeline.
    /// </summary>
    public SoapPipelineBuilder<TClient> AddResilience()
    {
        ResilientSoapMiddleware<TClient>.Add(services);
        return this;
    }

    /// <summary>
    /// Adds caching middleware to the pipeline.
    /// </summary>
    public SoapPipelineBuilder<TClient> AddCache(TimeSpan cacheDuration)
    {
        CachingSoapMiddleware<TClient>.Add(services, cacheDuration);
        return this;
    }

    /// <summary>
    /// Adds a custom middleware implementation to the pipeline.
    /// </summary>
    public SoapPipelineBuilder<TClient> AddMiddleware<TMiddleware>()
        where TMiddleware : SoapMiddleware<TClient>
    {
        services.AddScoped<SoapMiddleware<TClient>, TMiddleware>();
        return this;
    }
}
