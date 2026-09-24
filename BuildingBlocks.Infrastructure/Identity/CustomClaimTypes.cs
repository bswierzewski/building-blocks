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

    /// <summary>Tenant identifier from the user's Clerk public metadata ('tenant_id' in JWT and metadata).</summary>
    public const string TenantId = "tenant_id";

}
