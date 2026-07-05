using System.ServiceModel;

namespace BuildingBlocks.Soap.Abstractions;

/// <summary>
/// Base class for SOAP pipeline middleware.
/// Each middleware can wrap the next step in the pipeline and apply a cross-cutting concern.
/// </summary>
/// <typeparam name="TClient">The WCF client type.</typeparam>
public abstract class SoapMiddleware<TClient> where TClient : ICommunicationObject
{
    /// <summary>
    /// Executes the middleware and optionally invokes the next pipeline step.
    /// </summary>
    /// <typeparam name="TResult">The type returned by the SOAP operation.</typeparam>
    /// <param name="context">Operation metadata shared across the pipeline.</param>
    /// <param name="next">The next step in the pipeline.</param>
    /// <param name="ct">Cancellation token.</param>
    public abstract Task<TResult> InvokeAsync<TResult>(
        SoapCallContext context,
        Func<CancellationToken, Task<TResult>> next,
        CancellationToken ct = default);
}
