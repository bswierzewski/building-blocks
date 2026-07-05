namespace BuildingBlocks.Core.Exceptions;

/// <summary>
/// Exception thrown when user is authenticated but not authorized to access a resource.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("Dostęp do tego zasobu jest zabroniony.") { }

    public ForbiddenAccessException(string message)
        : base(message) { }

    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException) { }
}
