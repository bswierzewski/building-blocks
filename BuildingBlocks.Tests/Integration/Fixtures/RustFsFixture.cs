using Amazon.Runtime;
using Amazon.S3;
using BuildingBlocks.Storage.Options;
using BuildingBlocks.Storage.Services;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace BuildingBlocks.Tests.Integration.Fixtures;

/// <summary>
/// Starts RustFS, the same S3-compatible store the local environment uses, with one empty bucket.
/// </summary>
public sealed class RustFsFixture : IAsyncLifetime
{
    private const int S3Port = 9000;
    private const string AccessKey = "test-access-key";
    private const string SecretKey = "test-secret-key";
    private const string DefaultBucket = "tests";

    private readonly IContainer _container = new ContainerBuilder("rustfs/rustfs:1.0.3")
        .WithPortBinding(S3Port, assignRandomHostPort: true)
        .WithEnvironment("RUSTFS_ACCESS_KEY", AccessKey)
        .WithEnvironment("RUSTFS_SECRET_KEY", SecretKey)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPort(S3Port)
                .ForPath("/health")))
        .Build();

    /// <summary>
    /// Options pointing at the running container; every call returns a copy, so a test can change it freely.
    /// </summary>
    public StorageOptions CreateOptions(string bucket = DefaultBucket) => new()
    {
        ServiceUrl = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(S3Port)}",
        AccessKey = AccessKey,
        SecretKey = SecretKey,
        Bucket = bucket
    };

    public S3FileStorage CreateStorage(string bucket = DefaultBucket)
        => new(Microsoft.Extensions.Options.Options.Create(CreateOptions(bucket)));

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        // The application never creates buckets (a real store has them provisioned), so the fixture does it.
        var options = CreateOptions();
        using var client = new AmazonS3Client(
            new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            new AmazonS3Config { ServiceURL = options.ServiceUrl, ForcePathStyle = true });

        await client.PutBucketAsync(DefaultBucket);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
