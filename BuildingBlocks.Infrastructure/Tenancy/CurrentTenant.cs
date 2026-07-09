using BuildingBlocks.Core.Interfaces;
using BuildingBlocks.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Tenancy;

/// <summary>
/// Reads the current tenant context directly from JWT claims.
/// </summary>
public sealed class CurrentTenant(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public Guid? Id => Guid.TryParse(
        httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimTypes.TenantId)?.Value,
        out var tenantId)
            ? tenantId
            : null;

    public bool IsAvailable => Id.HasValue;
}
