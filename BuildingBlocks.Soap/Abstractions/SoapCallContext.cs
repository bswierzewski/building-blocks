using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BuildingBlocks.Soap.Abstractions;

/// <summary>
/// Carries operation metadata through the SOAP pipeline.
/// Middleware can use it to distinguish calls, for example for per-operation caching.
/// </summary>
public sealed record SoapCallContext(string OperationName, string? CacheKey = null)
{
    /// <summary>
    /// Creates a context with a stable cache key derived from the operation name and argument values.
    /// </summary>
    public static SoapCallContext ForCache(string operationName, params object?[] parameters)
    {
        var payload = JsonSerializer.Serialize(parameters);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        var hash = Convert.ToHexString(hashBytes);

        return new SoapCallContext(operationName, $"{operationName}:{hash}");
    }

    /// <summary>
    /// Creates a context that identifies an operation without enabling caching.
    /// </summary>
    public static SoapCallContext ForOperation(string operationName)
        => new(operationName);
}
