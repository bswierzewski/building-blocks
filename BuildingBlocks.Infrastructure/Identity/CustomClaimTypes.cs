namespace BuildingBlocks.Infrastructure.Identity;

/// <summary>
/// Custom JWT claim types used throughout the identity infrastructure.
/// Centralised here so every layer refers to the same string literals.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>Subject - the user's unique identifier ('sub' in JWT).</summary>
    public const string Sub = "sub";

    /// <summary>Email address assigned to the user ('email' in JWT).</summary>
    public const string Email = "email";

    /// <summary>Display name assigned to the user ('name' in JWT).</summary>
    public const string Name = "name";

    /// <summary>Roles assigned to the user ('roles' in JWT).</summary>
    public const string Roles = "roles";

    /// <summary>Permission codes derived from roles by <see cref="Services.PermissionClaimsTransformation"/>.</summary>
    public const string Permission = "permissions";

    /// <summary>Tenant identifier assigned by application metadata ('tenantId' in JWT).</summary>
    public const string TenantId = "tenantId";

    /// <summary>Tenant role assigned by application metadata ('tenantRole' in JWT).</summary>
    public const string TenantRole = "tenantRole";
}
