using BuildingBlocks.Pdf;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

/// <summary>
/// Starts Gotenberg, the same HTML to PDF service the local environment uses.
/// </summary>
public sealed class GotenbergFixture : IAsyncLifetime
{
    private const int Port = 3000;

    private readonly IContainer _container = new ContainerBuilder("gotenberg/gotenberg:8")
        .WithPortBinding(Port, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPort(Port)
                .ForPath("/health")))
        .Build();

    public Uri ServiceUrl => new($"http://{_container.Hostname}:{_container.GetMappedPublicPort(Port)}");

    /// <summary>
    /// Builds a container with the <see cref="PdfModule"/> registered, pointing at the running Gotenberg
    /// unless another address is given.
    /// </summary>
    public ServiceProvider CreateServices(Uri? serviceUrl = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gotenberg:ServiceUrl"] = (serviceUrl ?? ServiceUrl).ToString()
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        new PdfModule().AddServices(services, configuration);

        return services.BuildServiceProvider();
    }

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
