namespace BuildingBlocks.Core.Extensions;

/// <summary>
/// Provides helper methods for common string cleanup operations.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Trims text and collapses repeated whitespace into a single space.
    /// </summary>
    public static string? CleanWhitespace(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Trim()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Join(' ', parts);
    }
}
