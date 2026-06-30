using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

/// <summary>
/// Shared PostgreSQL test fixture that owns the Testcontainers instance and Respawn reset pipeline.
/// </summary>
public abstract class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private Respawner? _respawner;
    private NpgsqlConnection? _dbConnection;
    private readonly SemaphoreSlim _migrationLock = new(1, 1);
    private bool _migrationsApplied;

    /// <summary>
    /// Connection string exposed to integration hosts and direct database access in tests.
    /// </summary>
    public string ConnectionString => _dbContainer.GetConnectionString();

    /// <summary>
    /// Creates the PostgreSQL test container definition used by the shared database fixture.
    /// </summary>
    public DatabaseFixture()
    {
        _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("integration_tests_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    /// <summary>
    /// Starts the PostgreSQL container and opens the shared connection used by Respawn.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        _dbConnection = new NpgsqlConnection(ConnectionString);
        await _dbConnection.OpenAsync();
    }

    /// <summary>
    /// Applies application migrations once for the lifetime of this database fixture.
    /// </summary>
    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        if (_migrationsApplied)
            return;

        await _migrationLock.WaitAsync();
        try
        {
            // Another test class may have applied migrations while this caller was waiting for the lock.
            if (_migrationsApplied)
                return;

            await MigrateAsync(services);
            _migrationsApplied = true;
        }
        finally
        {
            _migrationLock.Release();
        }
    }

    /// <summary>
    /// Resets all included schemas while preserving ignored tables such as EF migration history.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection is null)
            throw new InvalidOperationException("The database fixture has not been initialized.");

        if (_respawner is null)
        {
            // Building blocks always include common infrastructure schemas; projects append their module schemas.
            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public", "wolverine", .. SchemasToInclude],
                TablesToIgnore = [new Table("public", "__EFMigrationsHistory"), .. TablesToIgnore]
            });
        }

        await _respawner.ResetAsync(_dbConnection);
    }

    /// <summary>
    /// Disposes the shared reset connection and stops the PostgreSQL container.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_dbConnection is not null)
            await _dbConnection.DisposeAsync();

        await _dbContainer.DisposeAsync();
        _migrationLock.Dispose();
    }

    /// <summary>
    /// Applies application-specific migrations for this database fixture.
    /// </summary>
    /// <param name="services">Service provider from the started application host.</param>
    protected abstract Task MigrateAsync(IServiceProvider services);

    /// <summary>
    /// Database schemas whose data should be reset between tests.
    /// </summary>
    protected abstract string[] SchemasToInclude { get; }

    /// <summary>
    /// Tables that should be preserved during a database reset.
    /// </summary>
    protected abstract Table[] TablesToIgnore { get; }
}
