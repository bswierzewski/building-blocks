using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Npgsql;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;

namespace BuildingBlocks.Infrastructure.Wolverine.Extensions;

/// <summary>
/// Provides Wolverine bootstrap extension methods for modular ASP.NET Core applications.
/// </summary>
public static class WolverineExtensions
{
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
