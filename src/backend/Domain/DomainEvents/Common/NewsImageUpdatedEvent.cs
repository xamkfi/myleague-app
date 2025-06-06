using Domain.DomainEvents;
using Domain.ValueObjects.Common;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a news article's image is added
/// </summary>
public class NewsImageUpdatedEvent : IDomainEvent
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
    /// Gets the image URL that was added
    /// </summary>
    public Uri ImageUrl { get; }

    /// <summary>
    /// Gets the date and time when the image was added
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsImageUpdatedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="imageUrl">The image URL that was added</param>
    /// <param name="updatedAt">The date and time when the image was added</param>
    public NewsImageUpdatedEvent(Guid newsId, Uri imageUrl, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        ImageUrl = imageUrl;
        UpdatedAt = updatedAt;
    }
} 
