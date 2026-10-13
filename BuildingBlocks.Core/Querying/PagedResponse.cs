namespace BuildingBlocks.Core.Querying;

/// <summary>One page of a list together with the numbers needed to build a pager.</summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
