using System.Diagnostics;
using BuildingBlocks.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Refit;

namespace BuildingBlocks.Infrastructure.Exceptions.Extensions;

/// <summary>
/// Provides extensions for enriching ProblemDetails responses with diagnostic data.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Maps known exceptions to a unified validation-style problem payload.
    /// </summary>
    public static HttpValidationProblemDetails ToProblemDetails(this Exception exception, IHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(env);

        return exception switch
        {
            ApiException ex => CreateProblemDetails(
                StatusCodes.Status502BadGateway,
                "Błąd usługi zewnętrznej",
                string.IsNullOrWhiteSpace(ex.Content) ? "Usługa zewnętrzna zwróciła błąd." : ex.Content),
            ApiRequestException => CreateProblemDetails(
                StatusCodes.Status502BadGateway,
                "Błąd usługi zewnętrznej",
                "Nie udało się połączyć z usługą zewnętrzną."),
            BadHttpRequestException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Nieprawidłowe żądanie",
                ex.Message),
            ValidationException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Błędy walidacji",
                ex.Message,
                ex.Errors),
            FluentValidation.ValidationException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Błędy walidacji",
                ex.Message,
                ex.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray())),
            NotFoundException ex => CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Nie znaleziono zasobu",
                ex.Message),
            DomainException ex => CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Naruszenie zasad biznesowych",
                ex.Message),
            UnauthorizedAccessException => CreateProblemDetails(
                StatusCodes.Status401Unauthorized,
                "Brak autoryzacji",
                exception.Message),
            ForbiddenAccessException => CreateProblemDetails(
                StatusCodes.Status403Forbidden,
                "Brak dostępu",
                exception.Message),
            _ => CreateProblemDetails(
                StatusCodes.Status500InternalServerError,
                "Błąd wewnętrzny serwera",
                env.IsDevelopment()
                    ? exception.Message
                    : "Wystąpił nieoczekiwany błąd. Skontaktuj się z pomocą techniczną.")
        };
    }

    /// <summary>
    /// Adds diagnostic information to ProblemDetails responses, including trace identifier, timestamp, and request path.
    /// </summary>
    public static ProblemDetailsOptions AddDiagnosticInformation(this ProblemDetailsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            var timestamp = DateTimeOffset.UtcNow;

            problem.Extensions.TryAdd("traceId", traceId);
            problem.Extensions.TryAdd("timestamp", timestamp);

            problem.Instance ??= context.HttpContext.Request.Path;

            if (problem is HttpValidationProblemDetails validationProblem && string.IsNullOrEmpty(problem.Detail))
            {
                var totalErrors = validationProblem.Errors.Sum(error => error.Value.Length);

                problem.Detail = totalErrors == 1
                    ? "Walidacja żądania nie powiodła się. Szczegóły znajdują się we właściwości 'errors'."
                    : $"Walidacja żądania nie powiodła się dla {totalErrors} błędów. Szczegóły znajdują się we właściwości 'errors'.";
            }
        };

        return options;
    }

    private static HttpValidationProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        return new HttpValidationProblemDetails(errors ?? new Dictionary<string, string[]>())
        {
            Status = status,
            Title = title,
            Detail = detail
        };
    }
}
