namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Provides access to the current tenant resolved for the request.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// Gets the identifier of the current tenant, when one is available.
    /// </summary>
    Guid? Id { get; }
}
