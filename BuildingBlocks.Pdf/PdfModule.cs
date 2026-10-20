using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Wolverine.Extensions;
using BuildingBlocks.Pdf.Health;
using BuildingBlocks.Pdf.Interfaces;
using BuildingBlocks.Pdf.Services;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Pdf;

/// <summary>
/// Registers HTML to PDF generation (Gotenberg) and rendering of Razor components to HTML.
/// </summary>
public sealed class PdfModule : IModule
{
    private const string SectionName = "Gotenberg";

    // Rendering a document takes much longer than a regular API call, and repeating it is safe.
    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(90);

    public string Name => "Pdf";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // @env: Gotenberg__ServiceUrl=
        // @env: Gotenberg__BasicAuthUsername=
        // @env: Gotenberg__BasicAuthPassword=
        //
        // Not validated on start: the migrator and the seeder load every module but have no Gotenberg configuration.
        // The API reads the options in InitializeAsync, so a bad configuration still stops it at startup.
        services.AddOptions<GotenbergSharpClientOptions>()
            .Bind(configuration.GetSection(SectionName))
            .Validate(options => options.ServiceUrl is { IsAbsoluteUri: true }, "Gotenberg:ServiceUrl must be an absolute URL.");

        // Rendering a document takes longer than the short limits of a regular API call, so this client has its own.
        services.AddGotenbergSharpClient()
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = AttemptTimeout;
                options.CircuitBreaker.SamplingDuration = AttemptTimeout * 2;
                options.TotalRequestTimeout.Timeout = AttemptTimeout * 2 + TimeSpan.FromSeconds(10);
                options.Retry.MaxRetryAttempts = 1;
            });

        services.AddTransient<IPdfGenerator, GotenbergPdfGenerator>();
        services.AddScoped<IHtmlComponentRenderer, HtmlComponentRenderer>();

        // The generator depends on the typed Gotenberg client, an opaque factory registration,
        // so Wolverine-generated code must resolve it from DI.
        services.AllowWolverineServiceLocationFor<IPdfGenerator>();

        // The PDF service is optional until something is configured, so an environment that has none yet can still run.
        if (IsConfigured(configuration))
            services.AddHealthChecks()
                .AddCheck<GotenbergHealthCheck>("gotenberg", tags: ["ready"]);
    }

    public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured(services.GetRequiredService<IConfiguration>()))
        {
            services.GetRequiredService<ILoggerFactory>()
                .CreateLogger<PdfModule>()
                .LogWarning(
                    "The PDF service is not configured (no {Section} settings), so generating PDF files will fail until it is.",
                    SectionName);

            return Task.CompletedTask;
        }

        // Once any setting is present, reading the value validates the options and fails fast on a missing service address.
        _ = services.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value;

        return Task.CompletedTask;
    }

    private static bool IsConfigured(IConfiguration configuration)
        => configuration.GetSection(SectionName).Exists();
}
