using BuildingBlocks.Core.Primitives;

namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Implemented by individual modules to register their role-to-permission mappings.
/// </summary>
public interface IRolePermissionProvider
{
    IEnumerable<Role> GetRolePermissions();
}
