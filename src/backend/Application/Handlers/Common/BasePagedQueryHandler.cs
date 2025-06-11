using Application.Common;
using Application.Services.Common;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Common;

/// <summary>
/// Base class for paginated query handlers that provides common pagination validation and logic
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result item type</typeparam>
public abstract class BasePagedQueryHandler<TQuery, TResult>
    where TQuery : class
{
    /// <summary>
    /// The pagination service for retrieving resource-specific settings
    /// </summary>
    protected readonly IPaginationService PaginationService;
    
    /// <summary>
    /// The logger for this handler
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the BasePagedQueryHandler
    /// </summary>
    /// <param name="paginationService">The pagination service</param>
    /// <param name="logger">The logger</param>
    protected BasePagedQueryHandler(IPaginationService paginationService, ILogger logger)
    {
        PaginationService = paginationService;
        _logger = logger;
    }

    /// <summary>
    /// Validates pagination parameters for a specific resource
    /// </summary>
    /// <param name="page">The page number</param>
    /// <param name="pageSize">The page size (0 means use default)</param>
    /// <param name="resourceKey">The resource key for pagination settings</param>
    /// <returns>Validation result with resolved page size if successful</returns>
    protected virtual Result<PaginationValidationResult> ValidatePaginationParameters(
        int page, int pageSize, string resourceKey)
    {
        // Validate page number
        if (page < 1)
        {
            _logger.LogWarning("Invalid page number: {Page} for resource: {ResourceKey}", page, resourceKey);
            return Result<PaginationValidationResult>.Failure("Page must be greater than 0");
        }

        // Resolve actual page size (convert 0 to default)
        int actualPageSize = PaginationService.ResolvePageSize(resourceKey, pageSize);

        // Validate page size bounds
        if (!PaginationService.IsValidPageSize(resourceKey, pageSize))
        {
            PaginationSettings settings = PaginationService.GetPaginationSettings(resourceKey);
            string errorMessage = pageSize == 0 
                ? $"Page size must be between {settings.MinPageSize} and {settings.MaxPageSize}"
                : $"Page size {pageSize} is invalid. Must be between {settings.MinPageSize} and {settings.MaxPageSize}";
            
            _logger.LogWarning("Invalid page size: {PageSize} for resource: {ResourceKey}. Min: {Min}, Max: {Max}", 
                pageSize, resourceKey, settings.MinPageSize, settings.MaxPageSize);
            
            return Result<PaginationValidationResult>.Failure(errorMessage);
        }

        _logger.LogDebug("Pagination validated for resource: {ResourceKey}. Page: {Page}, PageSize: {PageSize} (resolved: {ActualPageSize})", 
            resourceKey, page, pageSize, actualPageSize);

        return Result<PaginationValidationResult>.Success(
            new PaginationValidationResult(page, actualPageSize));
    }

    /// <summary>
    /// Gets pagination information for logging and response metadata
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <param name="page">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <returns>Pagination information</returns>
    protected virtual PaginationInfo GetPaginationInfo(string resourceKey, int page, int pageSize)
    {
        PaginationSettings settings = PaginationService.GetPaginationSettings(resourceKey);
        int actualPageSize = PaginationService.ResolvePageSize(resourceKey, pageSize);
        
        return new PaginationInfo(
            resourceKey,
            page,
            pageSize,
            actualPageSize,
            settings.DefaultPageSize,
            settings.MaxPageSize,
            settings.MinPageSize);
    }

    /// <summary>
    /// Creates a standard PagedResult using the existing PagedResult.Create method
    /// </summary>
    /// <param name="items">The items for the current page</param>
    /// <param name="totalCount">Total number of items across all pages</param>
    /// <param name="page">Current page number</param>
    /// <param name="actualPageSize">Actual page size used</param>
    /// <returns>PagedResult instance</returns>
    protected virtual PagedResult<TResult> CreatePagedResult(
        IEnumerable<TResult> items, int totalCount, int page, int actualPageSize)
    {
        return PagedResult<TResult>.Create(items, totalCount, page, actualPageSize);
    }
}

/// <summary>
/// Result of pagination validation containing resolved values
/// </summary>
/// <param name="Page">Validated page number</param>
/// <param name="ActualPageSize">Resolved actual page size (0 converted to default)</param>
public record PaginationValidationResult(int Page, int ActualPageSize);

/// <summary>
/// Information about pagination settings and resolved values for a request
/// </summary>
/// <param name="ResourceKey">The resource key</param>
/// <param name="RequestedPage">The requested page number</param>
/// <param name="RequestedPageSize">The requested page size (0 means default)</param>
/// <param name="ActualPageSize">The actual page size that will be used</param>
/// <param name="DefaultPageSize">The default page size for this resource</param>
/// <param name="MaxPageSize">The maximum allowed page size for this resource</param>
/// <param name="MinPageSize">The minimum allowed page size for this resource</param>
public record PaginationInfo(
    string ResourceKey,
    int RequestedPage,
    int RequestedPageSize,
    int ActualPageSize,
    int DefaultPageSize,
    int MaxPageSize,
    int MinPageSize); 