// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Domain.DomainEvents.Common;
using Domain.Enums.Common;
using Domain.EventSourcing;
using Domain.ValueObjects.Common;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents a news article entity that supports domain events for tracking changes.
    /// This class manages news content, metadata, categorization, and archiving functionality.
    /// </summary>
    public class NewsArticle : AggregateRoot
    {
        /// <summary>
        /// Gets the unique identifier of the news article.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the title of the news article. Limited to 200 characters.
        /// </summary>
        public string Title { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the main content of the news article in HTML format.
        /// </summary>
        public string ContentHtml { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the optional brief summary of the news article.
        /// </summary>
        public string? Summary { get; private set; }

        /// <summary>
        /// Gets the read-only list of image URLs associated with the news article.
        /// </summary>
        public IReadOnlyList<Uri> ImageUrls => _imageUrls.AsReadOnly();
        private readonly List<Uri> _imageUrls = new();

        /// <summary>
        /// Gets the optional author of the news article.
        /// </summary>
        public string? Author { get; private set; }

        /// <summary>
        /// Gets the UTC timestamp when the news article was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the UTC timestamp of the last update to the news article.
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the optional category classification of the news article.
        /// </summary>
        public NewsCategory? Category { get; private set; }

        /// <summary>
        /// Gets the optional sports-specific category of the news article.
        /// </summary>
        public SportsCategory? SportCategory { get; private set; }

        /// <summary>
        /// Gets the read-only list of tags associated with the news article.
        /// </summary>
        public IReadOnlyList<string> Tags => _tags.AsReadOnly();

        /// <summary>
        /// Gets whether the news article is archived.
        /// </summary>
        public bool IsArchived { get; private set; }

        private readonly List<string> _tags = new();
        private NewsArticle() { }

        /// <summary>
        /// Creates a new news article with the specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the news article.</param>
        /// <param name="title">The title of the news article (max 200 characters).</param>
        /// <param name="contentHtml">The HTML content of the news article.</param>
        /// <param name="author">The optional author of the news article.</param>
        /// <exception cref="ArgumentException">Thrown when title or content is empty or title exceeds 200 characters.</exception>
        public NewsArticle(Guid id, string title, string contentHtml, string? author = null)
        {
            Id = id;
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            Author = author;
            CreatedAt = DateTime.UtcNow;
            IsArchived = false;
            AddDomainEvent(new NewsArticleCreatedEvent(Id, Title, Author, CreatedAt));
        }

        /// <summary>
        /// Updates the content of the news article and raises a content update event.
        /// </summary>
        /// <param name="title">The new title (max 200 characters).</param>
        /// <param name="contentHtml">The new HTML content.</param>
        /// <param name="summary">The optional new summary.</param>
        /// <exception cref="ArgumentException">Thrown when title or content is empty or title exceeds 200 characters.</exception>
        public void UpdateContent(string title, string contentHtml, string? summary = null)
        {
            string oldTitle = Title;
            string oldContent = ContentHtml;
            string? oldSummary = Summary;
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            Summary = summary;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new NewsArticleContentUpdatedEvent(Id, oldTitle, Title, oldContent, ContentHtml, oldSummary, Summary, UpdatedAt.Value));
        }

        /// <summary>
        /// Adds an image URL to the news article and raises an image update event.
        /// </summary>
        /// <param name="imageUrl">The URL of the image to add.</param>
        /// <exception cref="ArgumentException">Thrown when the image URL is empty.</exception>
        public void SetImage(Uri imageUrl)
        {
            if (imageUrl == null)
                throw new ArgumentException("image URL cannot be empty.", nameof(imageUrl));

            _imageUrls.Add(imageUrl);
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new NewsArticleImageUpdatedEvent(Id ,imageUrl, UpdatedAt.Value));
        }

        /// <summary>
        /// Sets the news category and raises a category change event.
        /// </summary>
        /// <param name="category">The new category to set.</param>
        public void SetCategory(NewsCategory category)
        {
            NewsCategory? oldCategory = Category;
            Category = category;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new NewsArticleCategoryChangedEvent(Id, oldCategory, Category, UpdatedAt.Value));
        }

        /// <summary>
        /// Sets the sports category and raises a sports category change event.
        /// </summary>
        /// <param name="category">The new sports category to set.</param>
        public void SetSportCategory(SportsCategory category)
        {
            SportsCategory? oldCategory = SportCategory;
            SportCategory = category;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new NewsArticleSportCategoryChangedEvent(Id, oldCategory, SportCategory, UpdatedAt.Value));
        }

        /// <summary>
        /// Adds a tag to the news article if it doesn't already exist and raises a tag added event.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        /// <exception cref="ArgumentException">Thrown when the tag is empty.</exception>
        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag cannot be empty");
            if (_tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                return;
            _tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new NewsArticleTagAddedEvent(Id, tag, UpdatedAt.Value));
        }

        /// <summary>
        /// Removes a tag from the news article if it exists and raises a tag removed event.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        public void RemoveTag(string tag)
        {
            if (_tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new NewsArticleTagRemovedEvent(Id, tag, UpdatedAt.Value));
            }
        }

        /// <summary>
        /// Archives the news article if not already archived and raises an archive event.
        /// </summary>
        public void Archive()
        {
            if (IsArchived)
                return;
            IsArchived = true;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new NewsArticleArchivedEvent(Id, UpdatedAt.Value));
        }

        /// <summary>
        /// Restores the news article from archive if currently archived and raises a restore event.
        /// </summary>
        public void Restore()
        {
            if (!IsArchived)
                return;
            IsArchived = false;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new NewsArticleRestoredEvent(Id, UpdatedAt.Value));
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
                throw new ArgumentException("News title cannot be empty");
            if (title.Length > 200)
                throw new ArgumentException("News title cannot exceed 200 characters");
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
                throw new ArgumentException("News content cannot be empty");
            return contentHtml.Trim();
        }
    }
}
