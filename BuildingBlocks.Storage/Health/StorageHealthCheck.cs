using BuildingBlocks.Storage.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Storage.Health;

/// <summary>
/// Reports whether the object store is reachable and the configured bucket exists.
/// </summary>
public sealed class StorageHealthCheck(S3FileStorage storage) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await storage.BucketExistsAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("The storage bucket does not exist.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("The object store is not reachable.", ex);
        }
    }
}
