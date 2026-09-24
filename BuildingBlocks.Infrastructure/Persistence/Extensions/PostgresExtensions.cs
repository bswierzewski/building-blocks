using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Wolverine.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence.Extensions;

/// <summary>
/// Provides PostgreSQL-related service registration helpers for module persistence.
/// </summary>
public static class PostgresExtensions
{
    /// <summary>Per-schema EF migrations history table, named in snake_case like the rest of the database.</summary>
    public const string MigrationsHistoryTable = "__ef_migrations_history";

    /// <summary>
    /// Registers the shared PostgreSQL data source used by module DbContexts and Wolverine persistence.
    /// </summary>
    public static NpgsqlDataSource AddPostgresDataSource(this IServiceCollection services, IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            .EnableDynamicJson()
            .Build();

        services.TryAddSingleton(dataSource);

        return dataSource;
    }

    /// <summary>
    /// Registers a PostgreSQL-backed DbContext with audit interceptors.
    /// </summary>
    public static IServiceCollection AddPostgres<TDbContext>(this IServiceCollection services, string schema)
        where TDbContext : ModuleDbContext<TDbContext>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        services.TryAddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<TDbContext>((sp, options) =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            options
                .UseNpgsql(dataSource, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(MigrationsHistoryTable, schema))
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }

    /// <summary>
    /// Registers a PostgreSQL-backed DbContext whose <c>ITenanted</c> entities are managed by Wolverine's conjoined
    /// multi-tenancy: a <c>tenant_id</c> column, a global query filter on the ambient tenant, automatic stamping on
    /// insert and a guard against cross-tenant writes. The tenant comes from Wolverine tenant detection.
    /// </summary>
    public static IServiceCollection AddPostgresWithConjoinedTenancy<TDbContext>(this IServiceCollection services, string schema)
        where TDbContext : ModuleDbContext<TDbContext>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        services.TryAddSingleton<AuditableEntityInterceptor>();

        services.AddDbContextWithWolverineManagedConjoinedTenancy<TDbContext>(
            (options, dataSource) =>
            {
                options
                    .UseNpgsql((NpgsqlDataSource)dataSource, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(MigrationsHistoryTable, schema))
                    .UseSnakeCaseNamingConvention();
            },
            AutoCreate.None);

        return services;
    }
}
