namespace Application.Common;

/// <summary>
/// Represents a paginated result set with metadata about the pagination
/// </summary>
/// <typeparam name="T">The type of items in the result set</typeparam>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages)
{
    /// <summary>
    /// Gets whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets the number of items on the current page
    /// </summary>
    public int ItemCount => Items?.Count() ?? 0;

    /// <summary>
    /// Gets the starting item number for the current page (1-based)
    /// </summary>
    public int StartItem => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    /// <summary>
    /// Gets the ending item number for the current page (1-based)
    /// </summary>
    public int EndItem => TotalCount == 0 ? 0 : StartItem + ItemCount - 1;

    /// <summary>
    /// Creates a PagedResult with calculated total pages
    /// </summary>
    /// <param name="items">The items for the current page</param>
    /// <param name="totalCount">Total number of items across all pages</param>
    /// <param name="page">Current page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>A new PagedResult instance</returns>
    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        int totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResult<T>(items, totalCount, page, pageSize, totalPages);
    }

    /// <summary>
    /// Creates an empty PagedResult
    /// </summary>
    /// <param name="page">Current page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>An empty PagedResult</returns>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 10)
    {
        return new PagedResult<T>(Enumerable.Empty<T>(), 0, page, pageSize, 0);
    }
} 