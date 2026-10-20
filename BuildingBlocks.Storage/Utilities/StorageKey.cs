using System.Text.RegularExpressions;

namespace BuildingBlocks.Storage.Utilities;

/// <summary>
/// Builds and validates object keys, so callers cannot create keys that escape their area of the bucket.
/// </summary>
public static partial class StorageKey
{
    private const int MaxKeyLength = 1024;

    [GeneratedRegex("^[A-Za-z0-9._-]+$")]
    private static partial Regex SegmentPattern();

    /// <summary>
    /// Builds <c>segment/segment/{guid}</c>, e.g. <c>Create(tenantId, "archives")</c>.
    /// </summary>
    public static string Create(params string[] segments)
        => Prefix(segments) + Guid.CreateVersion7();

    /// <summary>
    /// Builds <c>segment/segment/</c>, e.g. to delete everything a tenant owns.
    /// </summary>
    public static string Prefix(params string[] segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        if (segments.Length == 0)
            throw new ArgumentException("At least one key segment is required.", nameof(segments));

        foreach (var segment in segments)
            EnsureValidSegment(segment, nameof(segments));

        return string.Join('/', segments) + "/";
    }

    /// <summary>
    /// Throws <see cref="ArgumentException"/> unless the key is a non-empty relative path without <c>.</c> or <c>..</c> segments.
    /// </summary>
    public static void EnsureValid(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("The storage key is empty.", nameof(key));

        if (key.Length > MaxKeyLength)
            throw new ArgumentException($"The storage key is longer than {MaxKeyLength} characters.", nameof(key));

        foreach (var segment in key.Split('/'))
            EnsureValidSegment(segment, nameof(key));
    }

    private static void EnsureValidSegment(string? segment, string paramName)
    {
        if (string.IsNullOrEmpty(segment) || segment is "." or ".." || !SegmentPattern().IsMatch(segment))
            throw new ArgumentException(
                $"'{segment}' is not a valid storage key segment (letters, digits, '.', '_' and '-' only, not '.' or '..').",
                paramName);
    }
}
