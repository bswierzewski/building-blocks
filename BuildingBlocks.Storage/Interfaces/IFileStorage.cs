using BuildingBlocks.Storage.Models;
using BuildingBlocks.Storage.Utilities;

namespace BuildingBlocks.Storage.Interfaces;

/// <summary>
/// Stores and reads files in an S3-compatible bucket. Keys come from <see cref="StorageKey"/>.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Writes the bytes from the current position of <paramref name="content"/> to the end, replacing any object under the key.
    /// The stream is not disposed.
    /// </summary>
    Task<StoredFile> PutAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the object for reading, or returns <see langword="null"/> when it does not exist.
    /// </summary>
    Task<FileContent?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the object. Deleting a missing object is not an error.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes every object whose key starts with the prefix (a trailing <c>/</c> is added when missing).
    /// </summary>
    Task DeleteByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a time-limited link the client can download from directly. Not implemented yet.
    /// </summary>
    Task<Uri> CreateDownloadUrlAsync(string key, TimeSpan lifetime, string? fileName = null, CancellationToken cancellationToken = default);
}
