using Gotenberg.Sharp.API.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Pdf.Health;

/// <summary>
/// Reports whether the Gotenberg service answers.
/// </summary>
public sealed class GotenbergHealthCheck(GotenbergSharpClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await client.GetGotenbergVersionAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("The PDF service is not reachable.", ex);
        }
    }
}
