using System.Reflection;
using BuildingBlocks.Hosting.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Hosting.Extensions;

public static class HostingExtensions
{
    private const string HealthEndpointPath = "/api/health";
    private const string AlivenessEndpointPath = "/api/alive";
    private const string VersionEndpointPath = "/api/version";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath))
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapGet(HealthEndpointPath, (HealthCheckService healthChecks, CancellationToken cancellationToken) =>
            GetApplicationHealth(healthChecks, cancellationToken))
        .WithName("GetApplicationHealth")
        .WithSummary("Get the application health status.")
        .WithDescription("Returns the aggregated health status of the application and its dependencies.")
        .WithTags("System")
        .Produces<ApplicationHealthResponse>(StatusCodes.Status200OK)
        .Produces<ApplicationHealthResponse>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet(AlivenessEndpointPath, (HealthCheckService healthChecks, CancellationToken cancellationToken) =>
            GetApplicationAliveness(healthChecks, cancellationToken))
        .WithName("GetApplicationAliveness")
        .WithSummary("Get the application liveness status.")
        .WithDescription("Returns the application's liveness status based on its self check.")
        .WithTags("System")
        .Produces<ApplicationHealthResponse>(StatusCodes.Status200OK)
        .Produces<ApplicationHealthResponse>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet(VersionEndpointPath, (IHostEnvironment environment) =>
            TypedResults.Ok(GetVersion(environment)))
        .WithName("GetVersion")
        .WithSummary("Get the application version.")
        .WithDescription("Returns the source revision used to build the application.")
        .WithTags("System")
        .AllowAnonymous()
        .Produces<VersionResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static VersionResponse GetVersion(IHostEnvironment environment)
    {
        var assembly = Assembly.Load(environment.ApplicationName);
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return new VersionResponse("unknown");
        }

        var sourceRevisionSeparator = informationalVersion.LastIndexOf('+');
        if (sourceRevisionSeparator < 0 || sourceRevisionSeparator == informationalVersion.Length - 1)
        {
            return new VersionResponse("unknown");
        }

        return new VersionResponse(informationalVersion[(sourceRevisionSeparator + 1)..]);
    }

    private static async Task<Results<Ok<ApplicationHealthResponse>, JsonHttpResult<ApplicationHealthResponse>, EmptyHttpResult>> GetApplicationHealth(
        HealthCheckService healthChecks,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await healthChecks.CheckHealthAsync(cancellationToken);
            var response = new ApplicationHealthResponse(report.Status.ToString());

            return report.Status == HealthStatus.Healthy
                ? TypedResults.Ok(response)
                : TypedResults.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return TypedResults.Empty;
        }
    }

    private static async Task<Results<Ok<ApplicationHealthResponse>, JsonHttpResult<ApplicationHealthResponse>, EmptyHttpResult>> GetApplicationAliveness(
        HealthCheckService healthChecks,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await healthChecks.CheckHealthAsync(
                registration => registration.Tags.Contains("live"),
                cancellationToken);

            var response = new ApplicationHealthResponse(report.Status.ToString());

            return report.Status == HealthStatus.Healthy
                ? TypedResults.Ok(response)
                : TypedResults.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return TypedResults.Empty;
        }
    }
}
