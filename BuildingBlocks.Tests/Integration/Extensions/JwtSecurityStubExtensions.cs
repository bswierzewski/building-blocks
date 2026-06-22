using System.Security.Claims;
using Alba;
using Alba.Security;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Identity;

namespace BuildingBlocks.Tests.Integration.Extensions;

/// <summary>
/// Helpers for expressing test authentication in terms of <see cref="ICurrentUser"/>.
/// </summary>
public static class JwtSecurityStubExtensions
{
    public static Scenario As(this Scenario scenario, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(currentUser);

        if (!currentUser.IsAuthenticated)
            return scenario;

        foreach (var claim in ToClaims(currentUser))
            scenario.WithClaim(claim);

        return scenario;
    }

    public static T As<T>(this T claimsTarget, ICurrentUser currentUser) where T : IHasClaims
    {
        ArgumentNullException.ThrowIfNull(claimsTarget);
        ArgumentNullException.ThrowIfNull(currentUser);

        if (!currentUser.IsAuthenticated)
            return claimsTarget;

        foreach (var claim in ToClaims(currentUser))
            claimsTarget.With(claim);

        return claimsTarget;
    }

    private static IEnumerable<Claim> ToClaims(ICurrentUser currentUser)
    {
        if (!string.IsNullOrWhiteSpace(currentUser.Id))
            yield return new Claim(CustomClaimTypes.Sub, currentUser.Id);

        if (currentUser.Status != UserStatus.None)
            yield return new Claim(CustomClaimTypes.UserStatus, currentUser.Status.ToString());

        foreach (var role in currentUser.Roles)
            yield return new Claim(CustomClaimTypes.Roles, role);

        foreach (var permission in currentUser.Permissions)
            yield return new Claim(CustomClaimTypes.Permission, permission);
    }
}