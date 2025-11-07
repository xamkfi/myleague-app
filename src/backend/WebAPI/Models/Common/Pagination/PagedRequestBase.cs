// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common.Pagination
{
    /// <summary>
    /// Base class for Paginated API responses
    /// </summary>
    public abstract record PagedRequestBase
    {
        /// <summary>
        /// Gets the current page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; init; } = 1;

        /// <summary>
        /// Gets the number of items per page
        /// </summary>
        [Range(0, 100, ErrorMessage = "Page size must be between 0 and 100")]
        public int PageSize { get; init; } = 0;

    }
}
