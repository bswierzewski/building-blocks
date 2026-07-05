using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// Provides helpers for coordinating background work with the host application lifetime.
/// </summary>
public static class HostApplicationLifetimeExtensions
{
    /// <summary>
    /// Waits until the application startup sequence completes or the stopping token is cancelled.
    /// </summary>
    public static async Task<bool> WaitForAppStartupAsync(
        this IHostApplicationLifetime lifetime,
        CancellationToken stoppingToken)
    {
        if (lifetime.ApplicationStarted.IsCancellationRequested)
            return true;

        var taskCompletionSource = new TaskCompletionSource();

        using var registration = lifetime.ApplicationStarted.Register(() => taskCompletionSource.TrySetResult());

        var cancellationTask = Task.Delay(Timeout.Infinite, stoppingToken);
        var completedTask = await Task.WhenAny(taskCompletionSource.Task, cancellationTask);

        return completedTask == taskCompletionSource.Task;
    }
}
