using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Identity.Services;

/// <summary>
/// Reads the current organization context directly from JWT claims.
/// </summary>
public sealed class CurrentOrganization(IHttpContextAccessor httpContextAccessor) : ICurrentOrganization
{
    public Guid? Id => Guid.TryParse(
        httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimTypes.OrganizationId)?.Value,
        out var organizationId)
            ? organizationId
            : null;
}
