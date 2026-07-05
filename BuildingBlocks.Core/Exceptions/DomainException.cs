namespace BuildingBlocks.Core.Exceptions;

/// <summary>
/// Represents an error that violates a domain business rule or constraint
/// </summary>
public class DomainException : Exception
{
    public DomainException() { }

    public DomainException(string message)
        : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
