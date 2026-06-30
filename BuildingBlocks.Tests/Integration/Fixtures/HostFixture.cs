using Alba;
using Alba.Security;
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
    private readonly JwtSecurityStub _jwtSecurity = new();
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
                builder.ConfigureServices((_, services) => configureServices(services));
            }, ConfigurationOverride.Create(configuration), _jwtSecurity);

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
