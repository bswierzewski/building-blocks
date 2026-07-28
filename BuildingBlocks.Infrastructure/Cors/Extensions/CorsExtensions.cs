using BuildingBlocks.Infrastructure.Cors.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Cors.Extensions;

/// <summary>
/// Registers and applies the API CORS policy for browser frontends.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds a restrictive CORS policy that permits only configured frontend origins.
    /// </summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CorsOptions.SectionName);
        var corsOptions = section.Get<CorsOptions>() ?? new CorsOptions();

        services.AddOptions<CorsOptions>()
            .Bind(section);

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.FrontendPolicyName, policy =>
            {
                if (corsOptions.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Applies the configured frontend CORS policy.
    /// </summary>
    public static WebApplication UseFrontendCors(this WebApplication app)
    {
        app.UseCors(CorsOptions.FrontendPolicyName);

        return app;
    }
}
