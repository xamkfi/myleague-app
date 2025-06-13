using Domain.DomainEvents;
using Domain.ValueObjects.Common;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a news article is created
/// </summary>
    public class NewsArticleCreatedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the date and time when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the ID of the news article
    /// </summary>
    public Guid NewsId { get; }

    /// <summary>
    /// Gets the title of the news article
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the author of the news article
    /// </summary>
    public string? Author { get; }

    /// <summary>
    /// Gets the date and time when the article was created
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsCreatedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="title">The title of the news article</param>
    /// <param name="author">The author of the news article</param>
    /// <param name="createdAt">The date and time when the article was created</param>
    public NewsArticleCreatedEvent(Guid newsId, string title, string? author, DateTime createdAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        Title = title;
        Author = author;
        CreatedAt = createdAt;
    }
}
