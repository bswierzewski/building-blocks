namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// Describes a permission published by a module.
/// </summary>
public sealed record Permission
{
    public Permission(string code, string description)
    {
        Code = NormalizeCode(code);
        Description = NormalizeDescription(description);
    }

    /// <summary>
    /// Gets the stable unique permission code, for example orders:read.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the business-facing description used by administration surfaces.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Normalizes a permission code for storage and comparison.
    /// </summary>
    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Kod uprawnienia nie może być pusty.");

        return code.ToLower().Trim();
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidOperationException("Opis uprawnienia nie może być pusty.");

        return description.Trim();
    }
}
