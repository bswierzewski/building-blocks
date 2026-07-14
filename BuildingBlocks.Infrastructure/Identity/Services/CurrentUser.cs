using System.Security.Claims;
using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// Reads the current user context directly from JWT claims.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public string Id => Principal.FindFirstValue(CustomClaimTypes.Sub) ?? string.Empty;

    public string? Email => Normalize(Principal.FindFirstValue(CustomClaimTypes.Email));

    public string? DisplayName => Normalize(Principal.FindFirstValue(CustomClaimTypes.Name));

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public IReadOnlySet<string> Roles =>
        Principal.FindAll(CustomClaimTypes.Roles).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> Permissions =>
        Principal.FindAll(CustomClaimTypes.Permission).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        Principal.HasClaim(CustomClaimTypes.Permission, permission);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
