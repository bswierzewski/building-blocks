using System.ServiceModel;
using BuildingBlocks.Soap.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace BuildingBlocks.Soap.Middleware;

/// <summary>
/// Resilient SOAP middleware that wraps operations with retry and timeout.
/// Retry: 3 attempts with exponential backoff starting from 1s.
/// Timeout: 15s per attempt.
/// Handles: CommunicationException, TimeoutException, and non-client FaultException.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public sealed class ResilientSoapMiddleware<TClient>
    : SoapMiddleware<TClient> where TClient : ICommunicationObject
{
    private readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(1),
            ShouldHandle = new PredicateBuilder()
                .Handle<CommunicationException>()
                .Handle<TimeoutException>()
                .Handle<FaultException>(ex => ex.Code?.Name != "Client")
        })
        .AddTimeout(TimeSpan.FromSeconds(15))
        .Build();

    /// <summary>
    /// Executes the next pipeline step through the resilience policy.
    /// </summary>
    public override async Task<TResult> InvokeAsync<TResult>(
        SoapCallContext context,
        Func<CancellationToken, Task<TResult>> next,
        CancellationToken ct = default)
    {
        return await _pipeline.ExecuteAsync(async token => await next(token), ct);
    }

    /// <summary>
    /// Registers resilience middleware for the specified SOAP client type.
    /// </summary>
    public static IServiceCollection Add(IServiceCollection services)
    {
        services.AddScoped<SoapMiddleware<TClient>, ResilientSoapMiddleware<TClient>>();
        return services;
    }
}
