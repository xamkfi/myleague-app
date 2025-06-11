// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Common
{
    /// <summary>
    /// Request model for creating a news article
    /// </summary>
    public record CreateNewsArticleRequest
    {
        /// <summary>
        /// Gets the title of the news article
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the Main image
        /// </summary>
        [Required(ErrorMessage = "MainImage is required")]
        public Uri? MainImage { get; init; }

        /// <summary>
        /// Gets the main content of the news article in HTML format
        /// </summary>
        [Required(ErrorMessage = "Content is required")]
        public string ContentHtml { get; init; } = string.Empty;

        /// <summary>
        /// Gets the optional brief summary of the news article
        /// </summary>
        [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
        public string? Summary { get; init; }

        /// <summary>
        /// Gets the list of image URLs associated with the news article
        /// </summary>
        public IReadOnlyList<string>? ImageUrls { get; init; }

        /// <summary>
        /// Gets the optional author of the news article
        /// </summary>
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string? Author { get; init; }

        /// <summary>
        /// Gets the optional category of the news article
        /// </summary>
        public string? Category { get; init; }

        /// <summary>
        /// Gets the optional sport category of the news article
        /// </summary>
        public string? SportCategory { get; init; }

        /// <summary>
        /// Gets the optional list of tags associated with the news article
        /// </summary>
        public IReadOnlyList<string>? Tags { get; init; }
    }

    /// <summary>
    /// Request model for updating a news article
    /// </summary>
    public record UpdateNewsArticleRequest
    {
        /// <summary>
        /// Gets the title of the news article
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the Main image
        /// </summary>
        public Uri? MainImage { get; init; }

        /// <summary>
        /// Gets the main content of the news article in HTML format
        /// </summary>
        [Required(ErrorMessage = "Content is required")]
        public string ContentHtml { get; init; } = string.Empty;

        /// <summary>
        /// Gets the optional brief summary of the news article
        /// </summary>
        [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
        public string? Summary { get; init; }

        /// <summary>
        /// Gets the list of image URLs associated with the news article
        /// </summary>
        public IReadOnlyList<string>? ImageUrls { get; init; }

        /// <summary>
        /// Gets the optional author of the news article
        /// </summary>
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string? Author { get; init; }

        /// <summary>
        /// Gets the optional category of the news article
        /// </summary>
        public string? Category { get; init; }

        /// <summary>
        /// Gets the optional sport category of the news article
        /// </summary>
        public string? SportCategory { get; init; }

        /// <summary>
        /// Gets the optional list of tags associated with the news article
        /// </summary>
        public IReadOnlyList<string>? Tags { get; init; }
    }

    /// <summary>
    /// Request model for setting an image for a news article
    /// </summary>
    public record SetNewsArticleImageRequest
    {
        /// <summary>
        /// Gets the image URL to set for the news article
        /// </summary>
        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string ImageUrl { get; init; } = string.Empty;
    }

    /// <summary>
    /// Request model for adding a tag to a news article
    /// </summary>
    public record AddNewsArticleTagRequest
    {
        /// <summary>
        /// Gets the tag to add to the news article
        /// </summary>
        [Required(ErrorMessage = "Tag is required")]
        [StringLength(50, ErrorMessage = "Tag cannot exceed 50 characters")]
        public string Tag { get; init; } = string.Empty;
    }

    /// <summary>
    /// Request model for removing a tag from a news article
    /// </summary>
    public record RemoveNewsArticleTagRequest
    {
        /// <summary>
        /// Gets the tag to remove from the news article
        /// </summary>
        [Required(ErrorMessage = "Tag is required")]
        [StringLength(50, ErrorMessage = "Tag cannot exceed 50 characters")]
        public string Tag { get; init; } = string.Empty;
    }

    /// <summary>
    /// Request model for searching news articles
    /// </summary>
    public record SearchNewsArticlesRequest
    {
        /// <summary>
        /// Gets the search term to find news articles
        /// </summary>
        [Required(ErrorMessage = "Search term is required")]
        [MinLength(2, ErrorMessage = "Search term must be at least 2 characters")]
        public string SearchTerm { get; init; } = string.Empty;
    }

    /// <summary>
    /// Request model for getting paginated news articles
    /// </summary>
    public record GetNewsArticlesRequest
    {
        /// <summary>
        /// Gets the page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; init; } = 1;

        /// <summary>
        /// Gets the number of items per page
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; init; } = 10;

        /// <summary>
        /// Gets the optional category filter
        /// </summary>
        public string? Category { get; init; }

        /// <summary>
        /// Gets the optional sport category filter
        /// </summary>
        public string? SportCategory { get; init; }

        /// <summary>
        /// Gets the optional search term
        /// </summary>
        [MinLength(2, ErrorMessage = "Search term must be at least 2 characters")]
        public string? Search { get; init; }

        /// <summary>
        /// Gets the optional author filter
        /// </summary>
        public string? Author { get; init; }

        /// <summary>
        /// Gets whether to include archived articles
        /// </summary>
        public bool IncludeArchived { get; init; } = false;
    }

    /// <summary>
    /// Request model for getting recent news articles
    /// </summary>
    public record GetRecentNewsArticlesRequest
    {
        /// <summary>
        /// Gets the number of recent articles to retrieve
        /// </summary>
        [Range(1, 50, ErrorMessage = "Count must be between 1 and 50")]
        public int Count { get; init; } = 10;

        /// <summary>
        /// Gets whether to include archived articles
        /// </summary>
        public bool IncludeArchived { get; init; } = false;
    }
} 
