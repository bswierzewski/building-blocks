using System.Diagnostics;
using System.Net;
using System.Text.Json;
using BuildingBlocks.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Refit;

namespace BuildingBlocks.Infrastructure.Exceptions.Extensions;

/// <summary>
/// One place for how the API reports errors: every failure is a ProblemDetails (RFC 7807).
/// </summary>
/// <remarks>
/// <see cref="ToProblemDetails" /> only decides the status and the message: our own exceptions (Domain, NotFound,
/// Forbidden) return their Polish message, everything else stays in the logs. <see cref="AddDiagnosticInformation" />
/// then gives every response, ours or the framework's, the Polish title and, when there is no message, the Polish
/// detail for its status. See Docs/error-handling.md.
/// </remarks>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Polish texts per HTTP status: a short heading (Title) and a full sentence (Detail) that ends with a period.
    /// Add a status here to translate it.
    /// </summary>
    private static readonly Dictionary<int, (string Title, string Detail)> StatusTexts = new()
    {
        [StatusCodes.Status400BadRequest] = ("Nieprawidłowe żądanie", "Żądanie ma nieprawidłowy format lub dane."),
        [StatusCodes.Status401Unauthorized] = ("Brak autoryzacji", "Zaloguj się, aby wykonać tę operację."),
        [StatusCodes.Status403Forbidden] = ("Brak dostępu", "Nie masz uprawnień do tej operacji."),
        [StatusCodes.Status404NotFound] = ("Nie znaleziono zasobu", "Nie znaleziono żądanego zasobu."),
        [StatusCodes.Status405MethodNotAllowed] = ("Metoda niedozwolona", "Ta metoda nie jest obsługiwana dla tego adresu."),
        [StatusCodes.Status406NotAcceptable] = ("Nieobsługiwany format odpowiedzi", "Serwer nie potrafi zwrócić odpowiedzi w żądanym formacie."),
        [StatusCodes.Status408RequestTimeout] = ("Przekroczono czas żądania", "Żądanie trwało zbyt długo. Spróbuj ponownie."),
        [StatusCodes.Status409Conflict] = ("Konflikt", "Operacja koliduje z aktualnym stanem danych."),
        [StatusCodes.Status413PayloadTooLarge] = ("Za duże żądanie", "Wysłane dane są zbyt duże."),
        [StatusCodes.Status415UnsupportedMediaType] = ("Nieobsługiwany typ danych", "Ten typ danych nie jest obsługiwany."),
        [StatusCodes.Status422UnprocessableEntity] = ("Nie można przetworzyć danych", "Dane mają poprawny format, ale nie można ich przetworzyć."),
        [StatusCodes.Status429TooManyRequests] = ("Zbyt wiele żądań", "Wykonano zbyt wiele żądań. Spróbuj ponownie za chwilę."),
        [StatusCodes.Status500InternalServerError] = ("Błąd wewnętrzny serwera", "Wystąpił nieoczekiwany błąd. Skontaktuj się z pomocą techniczną."),
        [StatusCodes.Status502BadGateway] = ("Błąd usługi zewnętrznej", "Usługa zewnętrzna zwróciła błąd. Spróbuj ponownie za chwilę."),
        [StatusCodes.Status503ServiceUnavailable] = ("Usługa niedostępna", "Usługa jest chwilowo niedostępna. Spróbuj ponownie za chwilę."),
        [StatusCodes.Status504GatewayTimeout] = ("Przekroczono czas oczekiwania", "Usługa nie odpowiedziała na czas. Spróbuj ponownie za chwilę.")
    };

    /// <summary>
    /// Maps an exception to a status and, where it has one meant for users, a message. Known exceptions get their own
    /// status; everything else is a 500.
    /// </summary>
    public static HttpValidationProblemDetails ToProblemDetails(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            // Upstream services. Their bodies are logged, never returned. A conflict there is a conflict for the user too.
            ApiException { StatusCode: HttpStatusCode.Conflict } => Problem(StatusCodes.Status409Conflict),
            ApiException => Problem(StatusCodes.Status502BadGateway),
            ApiRequestException => Problem(StatusCodes.Status502BadGateway, "Nie udało się połączyć z usługą zewnętrzną."),
            TimeoutException => Problem(StatusCodes.Status504GatewayTimeout),

            // Malformed requests. Framework messages are English and internal, so only the status is used.
            BadHttpRequestException ex => Problem(ex.StatusCode is >= 400 and < 500 ? ex.StatusCode : StatusCodes.Status400BadRequest),
            JsonException => Problem(StatusCodes.Status400BadRequest, "Dane w żądaniu mają nieprawidłowy format."),

            // Validation: the field messages are already Polish (FluentValidation culture, WithMessage).
            ValidationException ex => Problem(StatusCodes.Status400BadRequest, errors: ex.Errors),
            FluentValidation.ValidationException ex => Problem(
                StatusCodes.Status400BadRequest,
                errors: ex.Errors
                    .GroupBy(failure => failure.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray())),

            // Our own exceptions: their Polish messages are written for users. UnauthorizedAccessException also comes
            // from the framework and the file system, so its message is never returned.
            NotFoundException ex => Problem(StatusCodes.Status404NotFound, ex.Message),
            DomainException ex => Problem(StatusCodes.Status400BadRequest, ex.Message),
            ForbiddenAccessException ex => Problem(StatusCodes.Status403Forbidden, ex.Message),
            UnauthorizedAccessException => Problem(StatusCodes.Status401Unauthorized),

            // Database. Concurrency derives from DbUpdateException, so it goes first.
            DbUpdateConcurrencyException => Problem(
                StatusCodes.Status409Conflict,
                "Dane zostały zmienione przez inną osobę. Odśwież widok i spróbuj ponownie."),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } => Problem(
                StatusCodes.Status409Conflict,
                "Taka wartość już istnieje."),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } } => Problem(
                StatusCodes.Status409Conflict,
                "Operacji nie można wykonać, bo dane są powiązane z innymi danymi."),

            // Everything else. The message is in the logs.
            _ => Problem(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Runs for every problem response, from the exception handler and from the framework alike: adds diagnostics,
    /// replaces the title with the Polish one for the status and fills in the detail when there is none.
    /// </summary>
    public static ProblemDetailsOptions AddDiagnosticInformation(this ProblemDetailsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;

            problem.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
            problem.Extensions.TryAdd("timestamp", DateTimeOffset.UtcNow);
            problem.Instance ??= context.HttpContext.Request.Path;

            if (problem.Status is { } status && StatusTexts.TryGetValue(status, out var texts))
            {
                problem.Title = texts.Title;

                if (string.IsNullOrEmpty(problem.Detail))
                    problem.Detail = texts.Detail;
            }
        };

        return options;
    }

    private static HttpValidationProblemDetails Problem(
        int status,
        string? detail = null,
        IDictionary<string, string[]>? errors = null)
        => new(errors ?? new Dictionary<string, string[]>()) { Status = status, Detail = detail };
}
