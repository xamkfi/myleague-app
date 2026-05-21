// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Models.Common
{
    /// <summary>
    /// Request model for getting paginated feedback
    /// </summary>
    public record GetFeedbackRequest : PagedRequestBase
    {
        // Unsure if this is needed
    }

    /// <summary>
    /// Request model for creating Feedback
    /// </summary>
    public record CreateFeedbackRequest
    {
        /// <summary>
        /// Gets the title of the feedback
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the HTML body content of the feedback
        /// </summary>
        [Required(ErrorMessage = "Body is required")]
        public string FeedbackBody { get; init; } = string.Empty;

        /// <summary>
        /// Gets the optional email of the feedback
        /// </summary>
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; init; }
    }
}
