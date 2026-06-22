namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Represents the admin-controlled approval state of a user.
/// The value is sourced from the <c>status</c> claim in the JWT,
/// which is populated from Clerk's public metadata by the JWT template.
/// </summary>
public enum UserStatus
{
    /// <summary>No <c>status</c> claim is present in the token. Used when the application does not require approval.</summary>
    None,

    /// <summary>The user has been approved by an admin and can access the system.</summary>
    Approved,
}
