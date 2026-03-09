// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents the content and metadata of a web page, including its slug, title, HTML content, and last
    /// modification date.
    /// </summary>
    public class PageContent : BaseEntity
    {
        public string PageSlug { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;

        public string ContentHtml { get; private set; } = string.Empty;

        public string? LastModifiedBy { get; private set; }

        /// <summary>
        /// Initializes a new instance of the PageContent class with the specified page slug, title, HTML content, and
        /// optional last modified by information.
        /// </summary>
        /// <param name="pageSlug">The unique identifier for the page. Used in URLs to reference the page content. Cannot be null.</param>
        /// <param name="title">The title of the page, displayed to users and used in navigation. Cannot be null.</param>
        /// <param name="contentHtml">The HTML content to render for the page. Cannot be null.</param>
        /// <param name="lastModifiedBy">The username of the individual who last modified the page content, or null if not applicable.</param>
        public PageContent(Guid id, string pageSlug, string title, string contentHtml, string? lastModifiedBy)
        {
            Id = id;
            PageSlug = pageSlug;
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            LastModifiedBy = lastModifiedBy;
        }

        /// <summary>
        /// Updates the page content with the specified title and HTML content, and records the user who last modified
        /// it.
        /// </summary>
        /// <param name="title">The new title for the content. Must meet the required format and cannot be null.</param>
        /// <param name="contentHtml">The HTML content to update. Must be valid and safe for display.</param>
        /// <param name="lastModifiedBy">The username of the individual who last modified the content. Can be null if not applicable.</param>
        public void UpdateContent(string title, string contentHtml, string? lastModifiedBy)
        {
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            LastModifiedBy = lastModifiedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates the news title.
        /// </summary>
        /// <param name="title">The title to validate.</param>
        /// <returns>The trimmed, valid title.</returns>
        /// <exception cref="ArgumentException">Thrown when title is empty or exceeds 200 characters.</exception>
        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Page title cannot be empty");
            if (title.Length > 200)
                throw new ArgumentException("Page title cannot exceed 200 characters");
            return title.Trim();
        }

        /// <summary>
        /// Validates the news content.
        /// </summary>
        /// <param name="contentHtml">The content to validate.</param>
        /// <returns>The trimmed, valid content.</returns>
        /// <exception cref="ArgumentException">Thrown when content is empty.</exception>
        private static string ValidateContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml))
                throw new ArgumentException("Page content cannot be empty");
            return contentHtml.Trim();
        }
    }
}


