// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Domain.Enums.Common;
using Domain.EventSourcing;
using Domain.ValueObjects.Common;

namespace Domain.Entities.Common
{
    public class News : AggregateRoot<NewsId>
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        public string Title { get; private set; } = string.Empty;
        public string ContentHtml { get; private set; } = string.Empty;
        public string? Summary { get; private set; }
        public string? ImageUrl { get; private set; }
        public string? Author { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public NewsCategory? Category { get; private set; }
        public IReadOnlyList<string> Tags => _tags.AsReadOnly();
        public bool IsArchived { get; private set; }
        private readonly List<string> _tags = new();
        private News() { }
        public News(NewsId id, string title, string contentHtml, string? author = null)
        {
            Id = id;
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            Author = author;
            CreatedAt = DateTime.UtcNow;
            IsArchived = false;
            RaiseDomainEvent(new NewsCreatedEvent(Id, Title, Author, CreatedAt));
        }
        public void UpdateContent(string title, string contentHtml, string? summary = null)
        {
            var oldTitle = Title;
            var oldContent = ContentHtml;
            var oldSummary = Summary;
            Title = ValidateTitle(title);
            ContentHtml = ValidateContent(contentHtml);
            Summary = summary;
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new NewsContentUpdatedEvent(Id, oldTitle, Title, oldContent, ContentHtml, oldSummary, Summary, UpdatedAt.Value));
        }
        public void SetImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new DomainException("Image URL cannot be empty");
            var oldImageUrl = ImageUrl;
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new NewsImageUpdatedEvent(Id, oldImageUrl, ImageUrl, UpdatedAt.Value));
        }
        public void SetCategory(NewsCategory category)
        {
            var oldCategory = Category;
            Category = category;
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new NewsCategoryChangedEvent(Id, oldCategory, Category, UpdatedAt.Value));
        }
        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new DomainException("Tag cannot be empty");
            if (_tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                return;
            _tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new NewsTagAddedEvent(Id, tag, UpdatedAt.Value));
        }
        public void RemoveTag(string tag)
        {
            if (_tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                UpdatedAt = DateTime.UtcNow;
                RaiseDomainEvent(new NewsTagRemovedEvent(Id, tag, UpdatedAt.Value));
            }
        }
        public void Archive()
        {
            if (IsArchived)
                return;
            IsArchived = true;
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new NewsArchivedEvent(Id, UpdatedAt.Value));
        }
        public void Restore()
        {
            if (!IsArchived)
                return;
            IsArchived = false;
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new NewsRestoredEvent(Id, UpdatedAt.Value));
        }
        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("News title cannot be empty");
            if (title.Length > 200)
                throw new DomainException("News title cannot exceed 200 characters");
            return title.Trim();
        }
        private static string ValidateContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml))
                throw new DomainException("News content cannot be empty");
            return contentHtml.Trim();
        }
        protected override void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
}
