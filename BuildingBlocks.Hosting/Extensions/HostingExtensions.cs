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
using BuildingBlocks.Hosting.Models;

namespace BuildingBlocks.Hosting.Extensions;

public static class HostingExtensions
{
    private const string HealthEndpointPath = "/api/health";
    private const string AlivenessEndpointPath = "/api/alive";

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
        .WithTags("System")
        .Produces<ApplicationHealthResponse>(StatusCodes.Status200OK)
        .Produces<ApplicationHealthResponse>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet(AlivenessEndpointPath, (HealthCheckService healthChecks, CancellationToken cancellationToken) =>
            GetApplicationAliveness(healthChecks, cancellationToken))
        .WithName("GetApplicationAliveness")
        .WithSummary("Get the application liveness status.")
        .WithTags("System")
        .Produces<ApplicationHealthResponse>(StatusCodes.Status200OK)
        .Produces<ApplicationHealthResponse>(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<Results<Ok<ApplicationHealthResponse>, JsonHttpResult<ApplicationHealthResponse>>> GetApplicationHealth(
        HealthCheckService healthChecks,
        CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        var response = new ApplicationHealthResponse(report.Status.ToString());

        return report.Status == HealthStatus.Healthy
            ? TypedResults.Ok(response)
            : TypedResults.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<Results<Ok<ApplicationHealthResponse>, JsonHttpResult<ApplicationHealthResponse>>> GetApplicationAliveness(
        HealthCheckService healthChecks,
        CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(
            registration => registration.Tags.Contains("live"),
            cancellationToken);

        var response = new ApplicationHealthResponse(report.Status.ToString());

        return report.Status == HealthStatus.Healthy
            ? TypedResults.Ok(response)
            : TypedResults.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
