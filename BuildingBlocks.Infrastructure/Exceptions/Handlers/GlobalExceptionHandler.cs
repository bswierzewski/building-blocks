using BuildingBlocks.Infrastructure.Exceptions.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Refit;

namespace BuildingBlocks.Infrastructure.Exceptions.Handlers;

/// <summary>
/// Turns unhandled exceptions into ProblemDetails responses and logs them.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The client went away; nobody is waiting for an answer.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return false;

        // Once the response has started its status and body can no longer change; let the server abort the request.
        if (httpContext.Response.HasStarted)
        {
            logger.LogError(exception, "Exception occurred after the response had started.");
            return false;
        }

        var problem = exception.ToProblemDetails();
        var status = problem.Status ?? StatusCodes.Status500InternalServerError;

        // Client mistakes (4xx) are expected traffic: a warning. Only server-side failures (5xx) are errors.
        var level = status >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning;

        // The upstream body is never returned to the client, so the log is the only place to read it.
        if (exception is ApiException apiException)
            logger.Log(level, exception, "Upstream call failed with {StatusCode}: {Content}", apiException.StatusCode, apiException.Content);
        else
            logger.Log(level, exception, "Request failed with {Status}: {Message}", status, exception.Message);

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }
}
