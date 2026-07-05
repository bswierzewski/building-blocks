namespace BuildingBlocks.Core.Interfaces;

/// <summary>
/// Represents a domain event that occurred in the system.
/// </summary>
public interface IEvent
{
    /// <summary>Gets the unique identifier of this event.</summary>
    Guid Id { get; }

    /// <summary>Gets the timestamp when this event occurred.</summary>
    DateTime OccurredOn { get; }
}
