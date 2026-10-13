using BuildingBlocks.Core.Querying;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Querying.Extensions;

public static class PagingExtensions
{
    /// <summary>Counts the items and reads the requested page of an already filtered and ordered query.</summary>
    public static async Task<PagedResponse<T>> ToPagedAsync<T>(
        this IQueryable<T> source,
        PagedQuery query,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var totalCount = await source.CountAsync(ct);
        var items = await source
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        return new PagedResponse<T>(items, query.Page, query.PageSize, totalCount, totalPages);
    }
}
