using BuildingBlocks.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically manages audit fields on IAuditable entities when saving changes.
/// </summary>
public sealed class AuditableEntityInterceptor(ICurrentUser currentUser, TimeProvider dateTime) : SaveChangesInterceptor
{
    /// <summary>
    /// Intercepts synchronous SaveChanges calls to update audit fields.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Intercepts asynchronous SaveChangesAsync calls to update audit fields.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates audit fields for all IAuditable entities that are being added or modified.
    /// </summary>
    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = dateTime.GetUtcNow();
                var currentUserId = currentUser.Id;

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.CreatedAt = utcNow;
                }

                entry.Entity.ModifiedBy = currentUserId;
                entry.Entity.ModifiedAt = utcNow;
            }
        }
    }
}

/// <summary>
/// Extension methods for EntityEntry to check for changes in owned entities.
/// </summary>
public static class EntityEntryExtensions
{
    /// <summary>
    /// Determines if any owned entities of this entity have been added or modified.
    /// </summary>
    public static bool HasChangedOwnedEntities(this EntityEntry entry)
    {
        return entry.References.Any(reference =>
            reference.TargetEntry is not null &&
            reference.TargetEntry.Metadata.IsOwned() &&
            (reference.TargetEntry.State == EntityState.Added ||
             reference.TargetEntry.State == EntityState.Modified ||
             reference.TargetEntry.HasChangedOwnedEntities()));
    }
}
