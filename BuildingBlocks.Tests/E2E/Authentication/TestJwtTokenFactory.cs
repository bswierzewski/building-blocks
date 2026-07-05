using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Tests.E2E.Authentication;

/// <summary>
/// Creates short-lived JWTs for end-to-end tests from an <see cref="ICurrentUser"/> shape.
/// </summary>
public static class TestJwtTokenFactory
{
    public const string Issuer = "bb-e2e-tests";
    public const string SigningKey = "bb-e2e-tests-signing-key-please-change-if-needed";

    public static string Create(ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        if (!currentUser.IsAuthenticated)
            throw new InvalidOperationException("Cannot create a JWT for an unauthenticated test user.");

        var now = DateTimeOffset.UtcNow;
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            claims: ToClaims(currentUser),
            notBefore: now.UtcDateTime,
            expires: now.AddHours(1).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static IEnumerable<Claim> ToClaims(ICurrentUser currentUser)
    {
        if (!string.IsNullOrWhiteSpace(currentUser.Id))
            yield return new Claim(CustomClaimTypes.Sub, currentUser.Id);

        foreach (var role in currentUser.Roles)
            yield return new Claim(CustomClaimTypes.Roles, role);

        foreach (var permission in currentUser.Permissions)
            yield return new Claim(CustomClaimTypes.Permission, permission);
    }
}
