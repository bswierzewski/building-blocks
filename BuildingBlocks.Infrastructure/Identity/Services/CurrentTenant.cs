using BuildingBlocks.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Identity.Services;

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
}
