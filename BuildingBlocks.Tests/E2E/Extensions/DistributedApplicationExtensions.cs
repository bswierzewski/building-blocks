using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Tests.E2E.Extensions;

/// <summary>
/// Extension methods for creating EF Core contexts from Aspire-managed resources.
/// </summary>
public static class DistributedApplicationExtensions
{
    /// <summary>
    /// Creates a DbContext instance using the connection string exposed by the specified Aspire resource.
    /// </summary>
    public static async Task<TDbContext> CreateDbContextAsync<TDbContext>(
        this DistributedApplication app,
        string resourceName)
        where TDbContext : DbContext
    {
        var connectionString = await app.GetConnectionStringAsync(resourceName)
            ?? throw new InvalidOperationException($"Resource '{resourceName}' did not expose a connection string.");

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return Activator.CreateInstance(typeof(TDbContext), options) as TDbContext
            ?? throw new InvalidOperationException($"Could not create {typeof(TDbContext).Name}. Ensure it has a constructor accepting DbContextOptions<{typeof(TDbContext).Name}>.");
    }
}
