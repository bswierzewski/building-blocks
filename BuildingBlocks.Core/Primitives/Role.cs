namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// Describes a role and the permissions it grants.
/// </summary>
public sealed record Role
{
    public Role(string name, IEnumerable<Permission> permissions)
    {
        Name = NormalizeName(name);
        Permissions = NormalizePermissions(permissions);
    }

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the permissions granted by the role.
    /// </summary>
    public IReadOnlyCollection<Permission> Permissions { get; }

    /// <summary>
    /// Normalizes a role name for storage and comparison.
    /// </summary>
    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Nazwa roli nie może być pusta.");

        return name.Trim();
    }

    private static IReadOnlyCollection<Permission> NormalizePermissions(IEnumerable<Permission> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        return [.. permissions
            .DistinctBy(permission => permission.Code, StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission.Code, StringComparer.Ordinal)];
    }
}
