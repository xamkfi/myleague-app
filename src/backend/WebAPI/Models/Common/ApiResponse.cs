using Application.Common;
using Domain.Common;

namespace WebAPI.Models.Common;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
public record ApiResponse
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the message associated with the response
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the collection of errors if the operation failed
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Creates a successful response
    /// </summary>
    /// <param name="message">The success message</param>
    /// <returns>A successful API response</returns>
    public static ApiResponse SuccessResponse(string message = "Operation completed successfully")
    {
        return new ApiResponse { Success = true, Message = message };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>An error API response</returns>
    public static ApiResponse ErrorResponse(string message)
    {
        return new ApiResponse { Success = false, Message = message, Errors = new List<string> { message } };
    }

    /// <summary>
    /// Creates an error response with multiple errors
    /// </summary>
    /// <param name="errors">The collection of error messages</param>
    /// <returns>An error API response</returns>
    public static ApiResponse ErrorResponse(List<string> errors)
    {
        return new ApiResponse
        {
            Success = false,
            Message = "Operation failed with errors",
            Errors = errors
        };
    }
}

/// <summary>
/// Generic API response wrapper with data payload
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public record ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// Gets the data payload
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    /// <param name="data">The data to include in the response</param>
    /// <param name="message">The success message</param>
    /// <returns>A successful API response with data</returns>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    /// <summary>
    /// Creates an error response without data
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>An error API response</returns>
    public static new ApiResponse<T> ErrorResponse(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = new List<string> { message } };
    }

    /// <summary>
    /// Creates an error response without data
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>An error API response</returns>
    public static new ApiResponse<T> ErrorResponse(string message, List<string> errors)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }

    /// <summary>
    /// Creates an error response with multiple errors
    /// </summary>
    /// <param name="errors">The collection of error messages</param>
    /// <returns>An error API response</returns>
    public static new ApiResponse<T> ErrorResponse(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Operation failed with errors",
            Errors = errors
        };
    }
}

/// <summary>
/// Paginated API response wrapper that includes pagination metadata
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public record PaginatedApiResponse<T> : ApiResponse<IEnumerable<T>>
{
    /// <summary>
    /// Gets the pagination metadata
    /// </summary>
    public PaginationMetadata Pagination { get; init; } = new();

    /// <summary>
    /// Creates a successful paginated response
    /// </summary>
    /// <param name="pagedResult">The paged result from the application layer</param>
    /// <param name="message">The success message</param>
    /// <returns>A successful paginated API response</returns>
    public static PaginatedApiResponse<T> SuccessResponse(PagedResult<T> pagedResult, string message = "Data retrieved successfully")
    {
        return new PaginatedApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = pagedResult.Items,
            Pagination = new PaginationMetadata
            {
                CurrentPage = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage,
                StartItem = pagedResult.StartItem,
                EndItem = pagedResult.EndItem
            }
        };
    }

    /// <summary>
    /// Creates an error paginated response
    /// </summary>
    /// <param name="message">The error message</param>
    /// <returns>An error paginated API response</returns>
    public static new PaginatedApiResponse<T> ErrorResponse(string message)
    {
        return new PaginatedApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }

    /// <summary>
    /// Creates an error paginated response with multiple errors
    /// </summary>
    /// <param name="errors">The collection of error messages</param>
    /// <returns>An error paginated API response</returns>
    public static new PaginatedApiResponse<T> ErrorResponse(List<string> errors)
    {
        return new PaginatedApiResponse<T>
        {
            Success = false,
            Message = "Operation failed with errors",
            Errors = errors
        };
    }
}

/// <summary>
/// Pagination metadata for API responses
/// </summary>
public record PaginationMetadata
{
    /// <summary>
    /// Gets the current page number (1-based)
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// Gets the number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Gets whether there is a next page
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Gets whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Gets the starting item number for the current page (1-based)
    /// </summary>
    public int StartItem { get; init; }

    /// <summary>
    /// Gets the ending item number for the current page (1-based)
    /// </summary>
    public int EndItem { get; init; }
} 
