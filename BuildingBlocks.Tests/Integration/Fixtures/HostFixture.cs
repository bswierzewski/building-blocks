using Alba;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

/// <summary>
/// Class fixture that owns a single Alba host instance for one test class.
/// </summary>
/// <typeparam name="TEntryPoint">Application entry point used to bootstrap the in-memory host.</typeparam>
public class HostFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    /// <summary>
    /// Settings file the test project copies to its output directory; loaded after every other source when present.
    /// </summary>
    public const string TestSettingsFile = "appsettings.Testing.json";

    private readonly SemaphoreSlim _hostLock = new(1, 1);
    private bool _started;

    /// <summary>
    /// Running Alba host shared by all tests in the current test class.
    /// </summary>
    public IAlbaHost Host { get; private set; } = default!;

    /// <summary>
    /// Defers host startup until the database connection string is available from the collection fixture.
    /// </summary>
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Starts the host once for the current test class.
    /// </summary>
    /// <param name="connectionString">Database connection string injected into host configuration.</param>
    /// <param name="configure">Test-class configuration overrides applied before the host is built.</param>
    /// <param name="configureServices">Test-class service overrides applied before the host is built.</param>
    public async Task StartAsync(
        string connectionString,
        Action<IDictionary<string, string?>> configure,
        Action<IServiceCollection> configureServices)
    {
        if (_started)
            return;

        await _hostLock.WaitAsync();
        try
        {
            // xUnit creates one class fixture, but the guard keeps startup idempotent if multiple tests race here.
            if (_started)
                return;

            var configuration = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString
            };
            configure(configuration);

            Host = await AlbaHost.For<TEntryPoint>(builder =>
            {
                // The last layer of settings: whatever the developer's machine provides (user secrets, local files) must not
                // reach the tests, so the test project pins the values they depend on in this file.
                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, TestSettingsFile), optional: true));

                builder.ConfigureServices((_, services) => configureServices(services));
            }, [ConfigurationOverride.Create(configuration)]);

            _started = true;
        }
        finally
        {
            _hostLock.Release();
        }
    }

    /// <summary>
    /// Disposes the Alba host and synchronization primitive owned by this class fixture.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Host is not null)
            await Host.DisposeAsync();

        _hostLock.Dispose();
    }
}
