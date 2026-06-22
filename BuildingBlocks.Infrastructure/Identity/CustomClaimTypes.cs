namespace BuildingBlocks.Infrastructure.Identity;

/// <summary>
/// Custom JWT claim types used throughout the identity infrastructure.
/// Centralised here so every layer refers to the same string literals.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>Subject - the user's unique identifier ('sub' in JWT).</summary>
    public const string Sub = "sub";

    /// <summary>Roles assigned to the user ('roles' in JWT).</summary>
    public const string Roles = "roles";

    /// <summary>Permission codes derived from roles by <see cref="Services.PermissionClaimsTransformation"/>.</summary>
    public const string Permission = "permissions";

    /// <summary>User approval status set by an admin in Clerk public metadata ('status' in JWT).</summary>
    public const string UserStatus = "status";
}