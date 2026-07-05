using System.ServiceModel;
using BuildingBlocks.Soap.Abstractions;

namespace BuildingBlocks.Soap;

/// <summary>
/// SOAP pipeline responsible for WCF client lifecycle management and middleware pipeline execution.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public sealed class SoapPipeline<TClient>(
    Func<TClient> clientFactory,
    IEnumerable<SoapMiddleware<TClient>> middlewares)
    where TClient : ICommunicationObject
{
    private readonly IReadOnlyList<SoapMiddleware<TClient>> _middlewares = [.. middlewares];

    /// <summary>
    /// Executes a SOAP operation that returns a result.
    /// </summary>
    public async Task<TResult> InvokeAsync<TResult>(
        Func<TClient, Task<TResult>> operation,
        SoapCallContext context,
        CancellationToken ct = default)
        => await InvokeAsync((client, _) => operation(client), context, ct);

    /// <summary>
    /// Executes a SOAP operation without a return value.
    /// </summary>
    public async Task InvokeAsync(
        Func<TClient, Task> operation,
        SoapCallContext context,
        CancellationToken ct = default)
        => await InvokeAsync<object?>(async client =>
        {
            await operation(client);
            return null;
        }, context, ct);

    /// <summary>
    /// Executes a cancellable SOAP operation without a return value.
    /// </summary>
    public async Task InvokeAsync(
        Func<TClient, CancellationToken, Task> operation,
        SoapCallContext context,
        CancellationToken ct = default)
        => await InvokeAsync<object?>(async (client, token) =>
        {
            await operation(client, token);
            return null;
        }, context, ct);

    /// <summary>
    /// Executes a cancellable SOAP operation that returns a result.
    /// </summary>
    public async Task<TResult> InvokeAsync<TResult>(
        Func<TClient, CancellationToken, Task<TResult>> operation,
        SoapCallContext context,
        CancellationToken ct = default)
    {
        var client = clientFactory();
        try
        {
            if (client.State == CommunicationState.Created)
                await Task.Factory.FromAsync(client.BeginOpen, client.EndOpen, null);

            Func<CancellationToken, Task<TResult>> pipeline = token => operation(client, token);

            foreach (var middleware in _middlewares)
            {
                var next = pipeline;
                pipeline = token => middleware.InvokeAsync(context, next, token);
            }

            return await pipeline(ct);
        }
        catch
        {
            if (client.State == CommunicationState.Faulted)
                client.Abort();

            throw;
        }
        finally
        {
            if (client.State == CommunicationState.Opened)
            {
                try
                {
                    await Task.Factory.FromAsync(client.BeginClose, client.EndClose, null);
                }
                catch
                {
                    client.Abort();
                }
            }
        }
    }
}
