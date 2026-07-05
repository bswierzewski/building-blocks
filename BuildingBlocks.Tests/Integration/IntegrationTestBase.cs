using Alba;
using BuildingBlocks.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Base class for Alba-backed integration tests that configures the test host and resets the shared database before each test initialization.
/// </summary>
/// <typeparam name="TEntryPoint">Application entry point used to bootstrap the in-memory host.</typeparam>
/// <typeparam name="TDatabaseFixture">Collection fixture type that owns the shared test database.</typeparam>
public abstract class IntegrationTestBase<TEntryPoint, TDatabaseFixture>(
    TDatabaseFixture databaseFixture,
    HostFixture<TEntryPoint> hostFixture) : IAsyncLifetime, IClassFixture<HostFixture<TEntryPoint>>
    where TEntryPoint : class
    where TDatabaseFixture : DatabaseFixture
{
    /// <summary>
    /// Running Alba host shared by all tests in the current test class.
    /// </summary>
    public IAlbaHost Host => hostFixture.Host;

    /// <summary>
    /// Services from the host shared by all tests in the current test class.
    /// </summary>
    protected IServiceProvider Services => Host.Services;

    /// <summary>
    /// Allows the test class to override configuration before its host is built.
    /// Called once for the lifetime of the test class.
    /// </summary>
    protected virtual void ConfigureHost(IDictionary<string, string?> configuration) { }

    /// <summary>
    /// Allows the test class to replace or extend registrations before its host is built.
    /// Called once for the lifetime of the test class.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// Runs after the database reset and lets a test class seed data or prepare per-test state.
    /// </summary>
    protected virtual Task BeforeEachAsync() => Task.CompletedTask;

    /// <summary>
    /// Runs after each test and can be used for additional cleanup.
    /// </summary>
    protected virtual Task AfterEachAsync() => Task.CompletedTask;

    /// <summary>
    /// Resets the database and invokes the test-specific initialization hook.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Host startup needs the database connection string, while migrations need the started host services.
        await hostFixture.StartAsync(
            databaseFixture.ConnectionString,
            ConfigureHost,
            ConfigureServices);
        await databaseFixture.ApplyMigrationsAsync(Host.Services);
        await databaseFixture.ResetDatabaseAsync();
        await BeforeEachAsync();
    }

    /// <summary>
    /// Invokes the per-test disposal hook. The class fixture disposes the host after the final test.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await AfterEachAsync();
    }
}
