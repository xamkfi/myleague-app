// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents a page content entity that manages static page content across the application.
    /// This class handles page titles, slug identifiers, and HTML content for static pages.
    /// </summary>
    public class PageContent : BaseEntity
    {
        /// <summary>
        /// Gets the unique page slug identifier (e.g., "terms-of-service", "privacy-policy").
        /// </summary>
        public string PageSlug { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the title of the page. Limited to 500 characters.
        /// </summary>
        public string Title { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the main content of the page in HTML format.
        /// </summary>
        public string ContentHtml { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the optional name of the user who last modified this page.
        /// </summary>
        public string? LastModifiedBy { get; private set; }

        /// <summary>
        /// Private constructor for ORM and serialization.
        /// </summary>
        private PageContent() { }

        /// <summary>
        /// Creates a new page content instance with the specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the page content.</param>
        /// <param name="pageSlug">The unique page slug identifier (e.g., "terms-of-service").</param>
        /// <param name="title">The title of the page (max 500 characters).</param>
        /// <param name="contentHtml">The HTML content of the page.</param>
        /// <param name="lastModifiedBy">The optional name of the user who modified this page.</param>
        /// <exception cref="ArgumentException">Thrown when pageSlug, title, or contentHtml is empty, or when pageSlug exceeds 200 characters or title exceeds 500 characters.</exception>
        public PageContent(Guid id, string pageSlug, string title, string contentHtml, string? lastModifiedBy = null)
        {
            Id = id;
            PageSlug = ValidatePageSlug(pageSlug);
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            LastModifiedBy = lastModifiedBy;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the page content (title and HTML content) and modifies the LastModifiedBy timestamp.
        /// </summary>
        /// <param name="title">The new title (max 500 characters).</param>
        /// <param name="contentHtml">The new HTML content.</param>
        /// <param name="lastModifiedBy">The optional name of the user making this update.</param>
        /// <exception cref="ArgumentException">Thrown when title or contentHtml is empty or exceeds character limits.</exception>
        public void UpdateContent(string title, string contentHtml, string? lastModifiedBy = null)
        {
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            LastModifiedBy = lastModifiedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates the page slug.
        /// </summary>
        /// <param name="pageSlug">The slug to validate.</param>
        /// <returns>The trimmed, valid slug.</returns>
        /// <exception cref="ArgumentException">Thrown when slug is empty or exceeds 200 characters.</exception>
        private static string ValidatePageSlug(string pageSlug)
        {
            if (string.IsNullOrWhiteSpace(pageSlug))
                throw new ArgumentException("Page slug cannot be empty", nameof(pageSlug));
            if (pageSlug.Length > 200)
                throw new ArgumentException("Page slug cannot exceed 200 characters", nameof(pageSlug));
            return pageSlug.Trim();
        }

        /// <summary>
        /// Validates the page title.
        /// </summary>
        /// <param name="title">The title to validate.</param>
        /// <returns>The trimmed, valid title.</returns>
        /// <exception cref="ArgumentException">Thrown when title is empty or exceeds 500 characters.</exception>
        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Page title cannot be empty", nameof(title));
            if (title.Length > 500)
                throw new ArgumentException("Page title cannot exceed 500 characters", nameof(title));
            return title.Trim();
        }

        /// <summary>
        /// Validates the page content.
        /// </summary>
        /// <param name="contentHtml">The content to validate.</param>
        /// <returns>The trimmed, valid content.</returns>
        /// <exception cref="ArgumentException">Thrown when content is empty.</exception>
        private static string ValidateContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml))
                throw new ArgumentException("Page content cannot be empty", nameof(contentHtml));
            return contentHtml.Trim();
        }
    }
}
