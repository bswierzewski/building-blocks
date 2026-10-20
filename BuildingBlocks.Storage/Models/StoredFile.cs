namespace BuildingBlocks.Storage.Models;

/// <summary>
/// Describes an object that was written to the store; modules keep it next to their own metadata.
/// </summary>
/// <param name="Key">Object key in the bucket.</param>
/// <param name="SizeBytes">Number of bytes written.</param>
/// <param name="ContentType">Media type the object was stored with.</param>
/// <param name="ETag">Entity tag returned by the store, without quotes.</param>
public sealed record StoredFile(string Key, long SizeBytes, string ContentType, string? ETag);
