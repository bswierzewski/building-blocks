using BuildingBlocks.Core.Interfaces;

namespace BuildingBlocks.Tests.Models;

/// <summary>
/// Lightweight test implementation of <see cref="ICurrentUser"/> for per-request authorization setup.
/// </summary>
public sealed class TestCurrentUser(
    string id = "test-user",
    bool isAuthenticated = true,
    UserStatus status = UserStatus.Approved,
    IEnumerable<string>? roles = null,
    IEnumerable<string>? permissions = null) : ICurrentUser
{
    public string Id { get; } = id;

    public bool IsAuthenticated { get; } = isAuthenticated;

    public UserStatus Status { get; } = status;

    public IReadOnlySet<string> Roles { get; } = (roles ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> Permissions { get; } = (permissions ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}