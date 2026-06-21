using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

/// <summary>
/// Shared PostgreSQL test fixture that owns the Testcontainers instance and Respawn reset pipeline.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private Respawner? _respawner;
    private NpgsqlConnection? _dbConnection;

    /// <summary>
    /// Connection string exposed to integration hosts and direct database access in tests.
    /// </summary>
    public string ConnectionString => _dbContainer.GetConnectionString();

    /// <summary>
    /// Database schemas whose data should be reset between tests.
    /// Override this property in a project-specific fixture when the application uses custom schemas.
    /// </summary>
    protected virtual string[] SchemasToInclude => ["public", "wolverine"];

    /// <summary>
    /// Tables that should be preserved during a database reset.
    /// Override this property in a project-specific fixture to preserve module migration history tables.
    /// </summary>
    protected virtual Table[] TablesToIgnore => [new("public", "__EFMigrationsHistory")];

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
    /// Resets all included schemas while preserving ignored tables such as EF migration history.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection is null)
            throw new InvalidOperationException("The database fixture has not been initialized.");

        if (_respawner is null)
        {
            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = SchemasToInclude,
                TablesToIgnore = TablesToIgnore
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
    }
}
