// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain.Common;

namespace WebAPI.Models.Common.Pagination
{
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
}
