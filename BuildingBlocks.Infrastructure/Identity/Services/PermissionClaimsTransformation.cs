using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// ASP.NET Core claims transformation that appends permission claims to the principal
/// based on the roles already present in the identity.
/// </summary>
public sealed class PermissionClaimsTransformation(RolePermissionService rolePermissionService) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var roles = principal.FindAll(CustomClaimTypes.Roles).Select(c => c.Value);
        var permissions = rolePermissionService.GetPermissionsForRoles(roles);

        var identity = new ClaimsIdentity();

        foreach (var permission in permissions)
        {
            if (!principal.HasClaim(CustomClaimTypes.Permission, permission))
                identity.AddClaim(new Claim(CustomClaimTypes.Permission, permission));
        }

        if (identity.Claims.Any())
            principal.AddIdentity(identity);

        return Task.FromResult(principal);
    }
}
