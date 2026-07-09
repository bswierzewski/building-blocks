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

    /// <summary>
    /// Gets a value indicating whether a tenant has been resolved for the current request.
    /// </summary>
    bool IsAvailable { get; }
}
