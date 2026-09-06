using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;

namespace BuildingBlocks.Infrastructure.Wolverine.Extensions;

/// <summary>
/// Marks a DI service type that Wolverine is explicitly allowed to resolve through service location
/// when its implementation is intentionally hidden behind an opaque factory registration.
/// </summary>
internal sealed record WolverineServiceLocationRegistration(Type ServiceType);

/// <summary>
/// Provides Wolverine bootstrap extension methods for modular ASP.NET Core applications.
/// </summary>
public static class WolverineExtensions
{
    /// <summary>
    /// Allows Wolverine to resolve <typeparamref name="TService"/> from the scoped service provider.
    /// Use this only for registrations intentionally backed by an opaque factory, such as typed HTTP clients.
    /// The marker must be added before the application configures Wolverine.
    /// </summary>
    public static IServiceCollection AllowWolverineServiceLocationFor<TService>(this IServiceCollection services)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(services);

        var serviceType = typeof(TService);
        var alreadyRegistered = services.Any(descriptor =>
            descriptor.ServiceType == typeof(WolverineServiceLocationRegistration) &&
            descriptor.ImplementationInstance is WolverineServiceLocationRegistration registration &&
            registration.ServiceType == serviceType);

        if (!alreadyRegistered)
            services.AddSingleton(new WolverineServiceLocationRegistration(serviceType));

        return services;
    }

    /// <summary>
    /// Registers Wolverine in metadata-only mode for scenarios such as build-time OpenAPI generation,
    /// where HTTP endpoints must be discovered without activating database-backed messaging infrastructure.
    /// </summary>
    public static void AddWolverine(
        this WebApplicationBuilder builder,
        IModule[] modules,
        Action<WolverineOptions>? configure = null)
    {
        builder.AddWolverine(modules, dataSource: null, configure);
    }

    /// <summary>
    /// Registers shared Wolverine infrastructure for the provided modules.
    /// </summary>
    public static void AddWolverine(
        this WebApplicationBuilder builder,
        IModule[] modules,
        NpgsqlDataSource? dataSource,
        Action<WolverineOptions>? configure = null)
    {
        builder.Host.UseWolverine(opts =>
        {
            // Enable FluentValidation integration so message and HTTP handler validation
            // failures are automatically converted to structured problem details responses.
            opts.UseFluentValidation();

            // Treat each handler type that matches a given message as a separate execution path
            // rather than chaining them into a single pipeline. Prevents unintentional fan-out.
            opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

            // Metadata-only modes such as build-time OpenAPI generation don't provision a database connection.
            // In those modes we still want Wolverine to discover HTTP endpoints, but we must skip the durable
            // outbox and EF transaction wiring because both require a live PostgreSQL-backed data source.
            if (dataSource is not null)
            {
                // Use the durable outbox pattern with PostgreSQL to ensure messages are not lost in the event of a failure
                opts.PersistMessagesWithPostgresql(dataSource, "wolverine");

                // Enlist Wolverine in EF Core transactions so that message dispatch and database
                // writes participate in the same unit of work and commit atomically.
                opts.UseEntityFrameworkCoreTransactions();

                // Automatically wrap every handler that opens a DbContext in the transactional
                // outbox policy without requiring per-handler opt-in attributes.
                opts.Policies.AutoApplyTransactions();
            }

            // Some integrations intentionally hide their implementation behind a DI factory
            // (for example Refit's HttpClientFactory registration). Only services explicitly
            // marked by their owning module are allowed to use service location during codegen.
            foreach (var serviceType in builder.Services
                         .Where(service => service.ServiceType == typeof(WolverineServiceLocationRegistration))
                         .Select(service => service.ImplementationInstance)
                         .OfType<WolverineServiceLocationRegistration>()
                         .Select(registration => registration.ServiceType)
                         .Distinct())
                opts.CodeGeneration.AlwaysUseServiceLocationFor(serviceType);

            // Scan each module assembly so Wolverine discovers its HTTP endpoints,
            // message handlers, and any module-specific middleware or policies.
            foreach (var module in modules)
                opts.Discovery.IncludeAssembly(module.GetType().Assembly);

            // Allow the caller to extend or override Wolverine options without modifying
            // this shared helper — useful for application-specific transports or policies.
            configure?.Invoke(opts);
        });

    }
}
