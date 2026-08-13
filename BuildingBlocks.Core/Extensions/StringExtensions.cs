namespace BuildingBlocks.Core.Extensions;

/// <summary>
/// Provides helper methods for common string cleanup operations.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns only ASCII digits from the supplied text.
    /// </summary>
    public static string KeepOnlyDigits(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Where(character => character is >= '0' and <= '9').ToArray());
    }

    /// <summary>
    /// Trims text and collapses repeated whitespace into a single space.
    /// </summary>
    public static string CleanWhitespace(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var parts = value.Trim()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Join(' ', parts);
    }

    /// <summary>
    /// Trims text and collapses repeated whitespace into a single space,
    /// returning <see langword="null"/> when no text remains.
    /// </summary>
    public static string? CleanWhitespaceOrNull(this string? value)
    {
        var cleaned = value.CleanWhitespace();
        return cleaned.Length == 0 ? null : cleaned;
    }
}
