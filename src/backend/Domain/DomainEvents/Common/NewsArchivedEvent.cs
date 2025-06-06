using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a news article is archived
/// </summary>
public class NewsArchivedEvent : IDomainEvent
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
    /// Gets the date and time when the article was archived
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsArchivedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="updatedAt">The date and time when the article was archived</param>
    public NewsArchivedEvent(Guid newsId, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        UpdatedAt = updatedAt;
    }
} 