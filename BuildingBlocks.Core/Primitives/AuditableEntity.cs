using BuildingBlocks.Core.Interfaces;

namespace BuildingBlocks.Core.Primitives;

/// <summary>
/// Base class for entities that track creation and modification audit information
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
    where TId : notnull
{
    /// <summary>When this entity was created (defaults to UTC now)</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>User ID who created this entity</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>When this entity was last modified</summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>User ID who last modified this entity</summary>
    public string? ModifiedBy { get; set; }
}
