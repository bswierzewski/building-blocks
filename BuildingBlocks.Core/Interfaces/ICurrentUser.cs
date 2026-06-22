namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Provides access to the current user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the identifier of the current user.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets a value indicating whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the admin-controlled approval status of the current user.
    /// </summary>
    UserStatus Status { get; }

    /// <summary>
    /// Gets the collection of roles assigned to the current user.
    /// </summary>
    IReadOnlySet<string> Roles { get; }

    /// <summary>
    /// Gets the effective permissions assigned to the current user.
    /// </summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>
    /// Determines whether the current user has the specified permission.
    /// </summary>
    bool HasPermission(string permission);
}