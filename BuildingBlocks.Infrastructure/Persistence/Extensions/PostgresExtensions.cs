using BuildingBlocks.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Persistence.Extensions;

/// <summary>
/// Provides PostgreSQL-related service registration helpers for module persistence.
/// </summary>
public static class PostgresExtensions
{
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

        services.TryAddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<TDbContext>((sp, options) =>
        {
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            options.AddInterceptors(auditableInterceptor);

            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            options.UseNpgsql(dataSource, npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema));
        });

        return services;
    }
}
