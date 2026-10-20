using System.Text.Json;
using BuildingBlocks.Storage.Options;
using BuildingBlocks.Storage.Services;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

/// <summary>
/// Starts SeaweedFS with its S3 gateway, the same S3-compatible store the local environment uses.
/// </summary>
public sealed class SeaweedFsFixture : IAsyncLifetime
{
    private const int S3Port = 8333;
    private const string AccessKey = "test-access-key";
    private const string SecretKey = "test-secret-key";

    // The identities file SeaweedFS reads to accept the test credentials (property names are its JSON keys).
    private static readonly byte[] S3Identities = JsonSerializer.SerializeToUtf8Bytes(new
    {
        identities = new[]
        {
            new
            {
                name = "tests",
                credentials = new[] { new { accessKey = AccessKey, secretKey = SecretKey } },
                actions = new[] { "Admin", "Read", "Write", "List", "Tagging" }
            }
        }
    });

    private readonly IContainer _container = new ContainerBuilder("chrislusf/seaweedfs:latest")
        .WithPortBinding(S3Port, assignRandomHostPort: true)
        .WithResourceMapping(S3Identities, "/etc/seaweedfs/s3.json")
        .WithCommand("server", "-dir=/data", "-s3", "-s3.config=/etc/seaweedfs/s3.json", "-volume.max=20")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPort(S3Port)
                .ForStatusCodeMatching(_ => true)))
        .Build();

    /// <summary>
    /// Options pointing at the running container; every call returns a copy, so a test can change it freely.
    /// </summary>
    public StorageOptions CreateOptions(string bucket = "tests") => new()
    {
        ServiceUrl = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(S3Port)}",
        AccessKey = AccessKey,
        SecretKey = SecretKey,
        Bucket = bucket,
        ForcePathStyle = true
    };

    public S3FileStorage CreateStorage(string bucket = "tests")
        => new(Microsoft.Extensions.Options.Options.Create(CreateOptions(bucket)));

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        using var storage = CreateStorage();
        await storage.EnsureBucketAsync();
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
