// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents static MAHL info page content managed through the CMS.
    /// </summary>
    public class InfoPageContent : BaseEntity
    {
        public string PageSlug { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string ContentHtml { get; private set; } = string.Empty;
        public string? LastModifiedBy { get; private set; }

        private InfoPageContent() { }

        public InfoPageContent(
            Guid id,
            string pageSlug,
            string title,
            string contentHtml,
            string? lastModifiedBy = null)
        {
            Id = id;
            PageSlug = ValidatePageSlug(pageSlug);
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            LastModifiedBy = lastModifiedBy;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateContent(string title, string contentHtml, string? lastModifiedBy = null)
        {
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            LastModifiedBy = lastModifiedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        private static string ValidatePageSlug(string pageSlug)
        {
            if (string.IsNullOrWhiteSpace(pageSlug))
            {
                throw new ArgumentException("Page slug cannot be empty", nameof(pageSlug));
            }

            if (pageSlug.Length > 200)
            {
                throw new ArgumentException("Page slug cannot exceed 200 characters", nameof(pageSlug));
            }

            return pageSlug.Trim();
        }

        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Page title cannot be empty", nameof(title));
            }

            if (title.Length > 500)
            {
                throw new ArgumentException("Page title cannot exceed 500 characters", nameof(title));
            }

            return title.Trim();
        }

        private static string ValidateContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml))
            {
                throw new ArgumentException("Page content cannot be empty", nameof(contentHtml));
            }

            return contentHtml.Trim();
        }
    }
}
