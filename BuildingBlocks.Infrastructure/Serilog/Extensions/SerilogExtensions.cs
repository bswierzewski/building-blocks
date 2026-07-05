using BuildingBlocks.Infrastructure.Serilog.Builders;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;

namespace BuildingBlocks.Infrastructure.Serilog.Extensions;

/// <summary>
/// Provides infrastructure defaults for configuring Serilog on the generic host.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog with infrastructure defaults and user-defined sink configuration.
    /// </summary>
    public static IHostBuilder UseSerilog(
        this IHostBuilder hostBuilder,
        Action<SerilogBuilder> configure)
    {
        return SerilogHostBuilderExtensions.UseSerilog(hostBuilder, (context, services, loggerConfiguration) =>
        {
            var builder = new SerilogBuilder(context.Configuration, context.HostingEnvironment);

            loggerConfiguration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Information);
            }
            else
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
            }

            configure(builder);

            foreach (var action in builder.GetConfigurationActions())
                action(loggerConfiguration);

            loggerConfiguration.ReadFrom.Configuration(context.Configuration);
        });
    }
}
