using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a news article's author is updated
/// </summary>
public class NewsArticleAuthorUpdatedEvent : IDomainEvent
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
    /// Gets the previous author
    /// </summary>
    public string? OldAuthor { get; }

    /// <summary>
    /// Gets the new author
    /// </summary>
    public string? NewAuthor { get; }

    /// <summary>
    /// Gets the date and time when the author was updated
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsArticleAuthorUpdatedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="oldAuthor">The previous author</param>
    /// <param name="newAuthor">The new author</param>
    /// <param name="updatedAt">The date and time when the author was updated</param>
    public NewsArticleAuthorUpdatedEvent(Guid newsId, string? oldAuthor, string? newAuthor, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        OldAuthor = oldAuthor;
        NewAuthor = newAuthor;
        UpdatedAt = updatedAt;
    }
} 