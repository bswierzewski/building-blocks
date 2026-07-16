namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Provides access to the current organization resolved for the request.
/// </summary>
public interface ICurrentOrganization
{
    /// <summary>
    /// Gets the identifier of the current organization, when one is available.
    /// </summary>
    Guid? Id { get; }
}
