using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Modules.Extensions;

/// <summary>
/// Provides shared service registration helpers for validated options binding.
/// </summary>
public static class OptionsExtensions
{
    /// <summary>
    /// Binds options from configuration, enables data annotation validation, and validates them on application startup.
    /// </summary>
    public static IServiceCollection AddValidatedOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string path)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(path))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Registers a custom options validator and applies the standard validated options binding pipeline.
    /// </summary>
    public static IServiceCollection AddValidatedOptions<TOptions, TValidator>(this IServiceCollection services, IConfiguration configuration, string path)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddSingleton<IValidateOptions<TOptions>, TValidator>();

        return services.AddValidatedOptions<TOptions>(configuration, path);
    }
}
