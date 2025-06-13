using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when an archived news article is restored
/// </summary>
    public class NewsArticleRestoredEvent : IDomainEvent
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
    /// Gets the date and time when the article was restored
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsArticleRestoredEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="updatedAt">The date and time when the article was restored</param>
    public NewsArticleRestoredEvent(Guid newsId, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        UpdatedAt = updatedAt;
    }
} 