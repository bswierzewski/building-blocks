namespace BuildingBlocks.Storage.Models;

/// <summary>
/// A readable object returned by the store. Dispose it to release the underlying connection.
/// </summary>
public sealed class FileContent(Stream content, string contentType, long length, IDisposable? owner = null) : IAsyncDisposable
{
    /// <summary>
    /// Stream with the object bytes, read once from the start.
    /// </summary>
    public Stream Content { get; } = content;

    public string ContentType { get; } = contentType;

    public long Length { get; } = length;

    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync();
        owner?.Dispose();
    }
}
