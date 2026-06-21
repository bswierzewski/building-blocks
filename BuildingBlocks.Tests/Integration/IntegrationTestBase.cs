using Alba;
using Alba.Security;
using BuildingBlocks.Infrastructure.Modules.Extensions;
using BuildingBlocks.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.Integration;

/// <summary>
/// Base class for Alba-backed integration tests that configures the test host and resets the shared database before each test initialization.
/// </summary>
/// <typeparam name="TEntryPoint">Application entry point used to bootstrap the in-memory host.</typeparam>
public abstract class IntegrationTestBase<TEntryPoint>(DatabaseFixture databaseFixture) : IAsyncLifetime where TEntryPoint : class
{
    private readonly DatabaseFixture _dbFixture = databaseFixture;
    private readonly JwtSecurityStub _jwtSecurity = new();

    /// <summary>
    /// Running Alba host for the current test instance.
    /// </summary>
    public IAlbaHost Host { get; private set; } = default!;

    /// <summary>
    /// Allows a test class to replace or extend DI registrations before the host is built.
    /// </summary>
    protected virtual void OnConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// Runs after the database reset and lets a test class seed data or prepare per-test state.
    /// </summary>
    protected virtual Task OnInitializeAsync(IServiceProvider services) => Task.CompletedTask;

    /// <summary>
    /// Runs before the host is disposed and can be used for additional cleanup.
    /// </summary>
    protected virtual Task OnDisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Resets the shared database fixture to a clean state for the current test.
    /// </summary>
    public Task ResetDatabaseAsync() => _dbFixture.ResetDatabaseAsync();

    /// <summary>
    /// Builds the Alba host, resets the database, and invokes the test-specific initialization hook.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = _dbFixture.ConnectionString
        };

        Host = await AlbaHost.For<TEntryPoint>(builder =>
        {
            builder.ConfigureServices((_, services) => OnConfigureServices(services));
        }, ConfigurationOverride.Create(configValues), _jwtSecurity);

        await Host.Services.ApplyModuleMigrationsAsync();
        await ResetDatabaseAsync();
        await OnInitializeAsync(Host.Services);
    }

    /// <summary>
    /// Invokes the disposal hook and tears down the Alba host.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await OnDisposeAsync();

        if (Host != null)
            await Host.DisposeAsync();
    }
}
