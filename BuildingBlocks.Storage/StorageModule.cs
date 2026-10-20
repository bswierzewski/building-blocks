using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Wolverine.Extensions;
using BuildingBlocks.Storage.Health;
using BuildingBlocks.Storage.Interfaces;
using BuildingBlocks.Storage.Options;
using BuildingBlocks.Storage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Storage;

/// <summary>
/// Registers the S3-backed file storage.
/// </summary>
public sealed class StorageModule : IModule
{
    public string Name => "Storage";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Not validated on start: the migrator and the seeder load every module but have no storage configuration.
        // The API reads the options in InitializeAsync, so a bad configuration still stops it at startup.
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddSingleton<S3FileStorage>();
        services.AddSingleton<IFileStorage>(provider => provider.GetRequiredService<S3FileStorage>());

        // The alias above is an opaque factory registration, so Wolverine-generated code must resolve it from DI.
        services.AllowWolverineServiceLocationFor<IFileStorage>();

        // Storage is optional until something is configured, so an environment that has no object store yet can still run.
        if (IsConfigured(configuration))
            services.AddHealthChecks()
                .AddCheck<StorageHealthCheck>("storage", tags: ["ready"]);
    }

    public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured(services.GetRequiredService<IConfiguration>()))
        {
            services.GetRequiredService<ILoggerFactory>()
                .CreateLogger<StorageModule>()
                .LogWarning(
                    "Storage is not configured (no {Section} settings), so storing files will fail until it is.",
                    StorageOptions.SectionName);

            return Task.CompletedTask;
        }

        // Once any setting is present, reading the value validates the options and fails fast on a missing key or bucket
        // name. The bucket itself is created outside the application (the cloud console, or the AppHost locally).
        _ = services.GetRequiredService<IOptions<StorageOptions>>().Value;

        return Task.CompletedTask;
    }

    private static bool IsConfigured(IConfiguration configuration)
        => configuration.GetSection(StorageOptions.SectionName).Exists();
}
