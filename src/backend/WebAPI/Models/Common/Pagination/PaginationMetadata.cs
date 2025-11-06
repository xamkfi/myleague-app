// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace WebAPI.Models.Common.Pagination
{
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
}
