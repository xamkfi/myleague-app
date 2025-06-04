using Domain.DomainEvents;
using Domain.ValueObjects.Common;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a tag is added to a news article
/// </summary>
public class NewsTagAddedEvent : IDomainEvent
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
    /// Gets the tag that was added
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// Gets the date and time when the tag was added
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsTagAddedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="tag">The tag that was added</param>
    /// <param name="updatedAt">The date and time when the tag was added</param>
    public NewsTagAddedEvent(Guid newsId, string tag, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        Tag = tag;
        UpdatedAt = updatedAt;
    }
} 