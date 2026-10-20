using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using BuildingBlocks.Storage.Exceptions;
using BuildingBlocks.Storage.Interfaces;
using BuildingBlocks.Storage.Models;
using BuildingBlocks.Storage.Options;
using BuildingBlocks.Storage.Utilities;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Storage.Services;

/// <summary>
/// <see cref="IFileStorage"/> on top of the AWS S3 SDK. Works with AWS S3 and S3-compatible services such as Backblaze B2.
/// </summary>
public sealed class S3FileStorage(IOptions<StorageOptions> options) : IFileStorage, IDisposable
{
    private const string DefaultContentType = "application/octet-stream";

    private readonly IOptions<StorageOptions> _options = options;
    private readonly Lazy<AmazonS3Client> _client = new(() => CreateClient(options.Value), LazyThreadSafetyMode.ExecutionAndPublication);

    private AmazonS3Client Client => _client.Value;

    private string Bucket => _options.Value.Bucket!;

    /// <inheritdoc />
    public async Task<StoredFile> PutAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        StorageKey.EnsureValid(key);
        ArgumentNullException.ThrowIfNull(content);

        var mediaType = string.IsNullOrWhiteSpace(contentType) ? DefaultContentType : contentType;

        // S3 needs the length up front, so a stream that cannot report it is spooled to a temporary file first.
        FileStream? spool = null;
        try
        {
            var body = content;
            if (!content.CanSeek)
            {
                spool = new FileStream(
                    Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 81920,
                    FileOptions.DeleteOnClose | FileOptions.Asynchronous);

                await content.CopyToAsync(spool, cancellationToken);
                spool.Position = 0;
                body = spool;
            }

            var size = body.Length - body.Position;

            var response = await ExecuteAsync("put", key, () => Client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = Bucket,
                    Key = key,
                    InputStream = body,
                    ContentType = mediaType,
                    AutoCloseStream = false
                },
                cancellationToken));

            return new StoredFile(key, size, mediaType, response.ETag?.Trim('"'));
        }
        finally
        {
            if (spool is not null)
                await spool.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async Task<FileContent?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.EnsureValid(key);

        var response = await ExecuteAsync<GetObjectResponse?>("get", key, async () =>
        {
            try
            {
                return await Client.GetObjectAsync(Bucket, key, cancellationToken);
            }
            catch (AmazonS3Exception ex) when (IsNotFound(ex))
            {
                return null;
            }
        });

        if (response is null)
            return null;

        return new FileContent(
            response.ResponseStream,
            response.Headers.ContentType ?? DefaultContentType,
            response.ContentLength,
            owner: response);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.EnsureValid(key);

        return await ExecuteAsync("exists", key, async () =>
        {
            try
            {
                await Client.GetObjectMetadataAsync(Bucket, key, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (IsNotFound(ex))
            {
                return false;
            }
        });
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.EnsureValid(key);

        await ExecuteAsync("delete", key, () => Client.DeleteObjectAsync(Bucket, key, cancellationToken));
    }

    /// <inheritdoc />
    public async Task DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // The prefix is validated as a key and always ends with '/', so "a/b" never matches "a/bc/...".
        StorageKey.EnsureValid(prefix.TrimEnd('/'));
        prefix = prefix.TrimEnd('/') + "/";

        string? continuationToken = null;
        do
        {
            var page = await ExecuteAsync("list", prefix, () => Client.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = Bucket,
                    Prefix = prefix,
                    ContinuationToken = continuationToken
                },
                cancellationToken));

            if (page.S3Objects is { Count: > 0 })
            {
                await ExecuteAsync("delete", prefix, () => Client.DeleteObjectsAsync(
                    new DeleteObjectsRequest
                    {
                        BucketName = Bucket,
                        Objects = page.S3Objects.Select(x => new KeyVersion { Key = x.Key }).ToList()
                    },
                    cancellationToken));
            }

            continuationToken = page.IsTruncated == true ? page.NextContinuationToken : null;
        }
        while (continuationToken is not null);
    }

    /// <inheritdoc />
    public Task<Uri> CreateDownloadUrlAsync(
        string key,
        TimeSpan lifetime,
        string? fileName = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Download links are not supported yet; stream the file through the API instead.");

    /// <summary>
    /// Checks that the configured bucket exists and the credentials can list it.
    /// </summary>
    public async Task<bool> BucketExistsAsync(CancellationToken cancellationToken = default)
        => await ExecuteAsync("check bucket", Bucket, async () =>
        {
            try
            {
                await Client.ListObjectsV2Async(
                    new ListObjectsV2Request { BucketName = Bucket, MaxKeys = 1 },
                    cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
            {
                return false;
            }
        });

    public void Dispose()
    {
        if (_client.IsValueCreated)
            _client.Value.Dispose();
    }

    private static AmazonS3Client CreateClient(StorageOptions options)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = options.ForcePathStyle ?? !string.IsNullOrWhiteSpace(options.ServiceUrl),

            // Some S3-compatible services reject the integrity checksums the SDK adds by default.
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
        };

        if (string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
        }
        else
        {
            config.ServiceURL = options.ServiceUrl;
            config.AuthenticationRegion = options.Region;
        }

        return new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config);
    }

    private static bool IsNotFound(AmazonS3Exception exception)
        => exception.StatusCode == HttpStatusCode.NotFound || exception.ErrorCode is "NoSuchKey" or "NotFound";

    private static async Task<T> ExecuteAsync<T>(string operation, string key, Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (ex is AmazonServiceException or AmazonClientException)
        {
            // The SDK has two unrelated base types: service errors (S3 said no) and client errors (no answer).
            throw new StorageException($"Storage operation '{operation}' failed for '{key}'.", ex);
        }
    }
}
