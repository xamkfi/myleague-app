using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a news article's main image is updated
/// </summary>
public class NewsArticleMainImageUpdatedEvent : IDomainEvent
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
    /// Gets the previous main image URL
    /// </summary>
    public Uri? OldMainImage { get; }

    /// <summary>
    /// Gets the new main image URL
    /// </summary>
    public Uri? NewMainImage { get; }

    /// <summary>
    /// Gets the date and time when the main image was updated
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsArticleMainImageUpdatedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="oldMainImage">The previous main image URL</param>
    /// <param name="newMainImage">The new main image URL</param>
    /// <param name="updatedAt">The date and time when the main image was updated</param>
    public NewsArticleMainImageUpdatedEvent(Guid newsId, Uri? oldMainImage, Uri? newMainImage, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        OldMainImage = oldMainImage;
        NewMainImage = newMainImage;
        UpdatedAt = updatedAt;
    }
} 