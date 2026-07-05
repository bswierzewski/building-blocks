using System.Collections.Frozen;
using BuildingBlocks.Core.Interfaces;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// Aggregates role-to-permission mappings from all registered <see cref="IRolePermissionProvider"/> instances
/// and exposes fast frozen lookups keyed by role name.
/// </summary>
public sealed class RolePermissionService(IEnumerable<IRolePermissionProvider> providers)
{
    private readonly FrozenDictionary<string, FrozenSet<string>> _rolePermissions = providers
            .SelectMany(p => p.GetRolePermissions())
            .GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(
                g => g.Key,
                g => g.SelectMany(r => r.Permissions.Select(p => p.Code))
                       .ToFrozenSet(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the set of permission codes granted by the specified role.
    /// Returns an empty set when the role is not registered.
    /// </summary>
    public FrozenSet<string> GetPermissionsForRole(string role) =>
        _rolePermissions.TryGetValue(role, out var permissions)
            ? permissions
            : Array.Empty<string>().ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the combined, deduplicated set of permission codes for the given roles.
    /// </summary>
    public IEnumerable<string> GetPermissionsForRoles(IEnumerable<string> roles) =>
        roles.SelectMany(GetPermissionsForRole).Distinct(StringComparer.OrdinalIgnoreCase);
}
