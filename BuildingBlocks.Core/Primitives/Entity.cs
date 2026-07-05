namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// Base class for all domain entities with identity
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>Gets the unique identifier of this entity</summary>
    public TId Id { get; init; } = default!;
}
