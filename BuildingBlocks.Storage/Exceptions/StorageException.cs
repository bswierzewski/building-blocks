namespace BuildingBlocks.Storage.Exceptions;

/// <summary>
/// Thrown when the object store fails. It is an infrastructure fault, not an error the user can fix.
/// </summary>
public sealed class StorageException : Exception
{
    public StorageException(string message)
        : base(message) { }

    public StorageException(string message, Exception innerException)
        : base(message, innerException) { }
}
