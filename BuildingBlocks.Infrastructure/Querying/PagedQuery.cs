using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Infrastructure.Querying;

/// <summary>
/// Base of list requests: which page to return. Filters and ordering belong to the request of each list, which
/// declares them (with enums, so clients get the values from the API contract) and applies them in its handler.
/// </summary>
public abstract class PagedQuery
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    /// <summary>Number of the page, starting with 1.</summary>
    [FromQuery(Name = "page")]
    public int Page { get; set; } = 1;

    /// <summary>Number of items on a page, at most <see cref="MaxPageSize"/>.</summary>
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = DefaultPageSize;
}
