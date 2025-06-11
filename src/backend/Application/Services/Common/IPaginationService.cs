namespace Application.Services.Common;

/// <summary>
/// Service for retrieving pagination settings for different resource types
/// </summary>
public interface IPaginationService
{
    /// <summary>
    /// Gets pagination settings for a specific resource by key
    /// </summary>
    /// <param name="resourceKey">The resource key (e.g., "News", "FloorballPlayers")</param>
    /// <returns>Pagination settings for the resource</returns>
    PaginationSettings GetPaginationSettings(string resourceKey);

    /// <summary>
    /// Gets pagination settings for a specific resource type
    /// </summary>
    /// <typeparam name="T">The resource type</typeparam>
    /// <returns>Pagination settings for the resource type</returns>
    PaginationSettings GetPaginationSettings<T>() where T : class;

    /// <summary>
    /// Gets the default page size for a specific resource
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <returns>Default page size</returns>
    int GetDefaultPageSize(string resourceKey);

    /// <summary>
    /// Gets the maximum allowed page size for a specific resource
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <returns>Maximum page size</returns>
    int GetMaxPageSize(string resourceKey);

    /// <summary>
    /// Gets the minimum allowed page size for a specific resource
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <returns>Minimum page size</returns>
    int GetMinPageSize(string resourceKey);

    /// <summary>
    /// Validates if a page size is within allowed bounds for a resource
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <param name="pageSize">The page size to validate (0 means use default)</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidPageSize(string resourceKey, int pageSize);

    /// <summary>
    /// Resolves the actual page size to use (converts 0 to default)
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <param name="requestedPageSize">The requested page size (0 means use default)</param>
    /// <returns>The actual page size to use</returns>
    int ResolvePageSize(string resourceKey, int requestedPageSize);
}

/// <summary>
/// Pagination settings for a resource
/// </summary>
/// <param name="DefaultPageSize">Default page size when none specified</param>
/// <param name="MaxPageSize">Maximum allowed page size</param>
/// <param name="MinPageSize">Minimum allowed page size</param>
public record PaginationSettings(
    int DefaultPageSize,
    int MaxPageSize,
    int MinPageSize); 