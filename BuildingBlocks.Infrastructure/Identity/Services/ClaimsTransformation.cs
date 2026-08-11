using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// Enriches authenticated principals with claims derived by the application.
/// Claims supplied directly by the JWT, including the current organization,
/// remain available without additional transformation.
/// </summary>
public sealed class ClaimsTransformation(RolePermissionService rolePermissionService)
    : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        AddPermissionClaims(principal);

        return Task.FromResult(principal);
    }

    private void AddPermissionClaims(ClaimsPrincipal principal)
    {
        var roles = principal.FindAll(CustomClaimTypes.Roles).Select(claim => claim.Value);
        var permissions = rolePermissionService.GetPermissionsForRoles(roles);
        var identity = new ClaimsIdentity();

        foreach (var permission in permissions)
        {
            if (!principal.HasClaim(CustomClaimTypes.Permission, permission))
                identity.AddClaim(new Claim(CustomClaimTypes.Permission, permission));
        }

        if (identity.Claims.Any())
            principal.AddIdentity(identity);
    }
}
