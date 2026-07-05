using System.Diagnostics;
using System.ServiceModel;
using BuildingBlocks.Soap.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Soap.Middleware;

/// <summary>
/// Logging SOAP middleware that logs operation start, completion, and failures.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public sealed class LoggingSoapMiddleware<TClient>(
    ILogger<LoggingSoapMiddleware<TClient>> logger)
    : SoapMiddleware<TClient> where TClient : ICommunicationObject
{
    private static readonly string ClientName = typeof(TClient).Name;

    /// <summary>
    /// Logs the execution time and failures for the next pipeline step.
    /// </summary>
    public override async Task<TResult> InvokeAsync<TResult>(
        SoapCallContext context,
        Func<CancellationToken, Task<TResult>> next,
        CancellationToken ct = default)
    {
        logger.LogInformation("SOAP {ClientName} {OperationName} - invoking", ClientName, context.OperationName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next(ct);
            stopwatch.Stop();
            logger.LogInformation("SOAP {ClientName} {OperationName} - completed in {ElapsedMs}ms", ClientName, context.OperationName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "SOAP {ClientName} {OperationName} - failed after {ElapsedMs}ms", ClientName, context.OperationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Registers logging middleware for the specified SOAP client type.
    /// </summary>
    public static IServiceCollection Add(IServiceCollection services)
    {
        services.AddScoped<SoapMiddleware<TClient>, LoggingSoapMiddleware<TClient>>();
        return services;
    }
}
