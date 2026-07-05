namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Provides audit trail information for entities that track creation and modification history.
/// </summary>
public interface IAuditable
{
    /// <summary>Gets or sets when this entity was created.</summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the user ID who created this entity.</summary>
    string CreatedBy { get; set; }

    /// <summary>Gets or sets when this entity was last modified.</summary>
    DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>Gets or sets the user ID who last modified this entity.</summary>
    string? ModifiedBy { get; set; }
}
