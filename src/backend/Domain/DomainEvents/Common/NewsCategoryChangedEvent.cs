using Domain.DomainEvents;
using Domain.Enums.Common;
using Domain.ValueObjects.Common;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a news article's category is changed
/// </summary>
public class NewsCategoryChangedEvent : IDomainEvent
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
    /// Gets the previous category
    /// </summary>
    public NewsCategory? OldCategory { get; }

    /// <summary>
    /// Gets the new category
    /// </summary>
    public NewsCategory? NewCategory { get; }

    /// <summary>
    /// Gets the date and time when the category was changed
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsCategoryChangedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="oldCategory">The previous category</param>
    /// <param name="newCategory">The new category</param>
    /// <param name="updatedAt">The date and time when the category was changed</param>
    public NewsCategoryChangedEvent(Guid newsId, NewsCategory? oldCategory, NewsCategory? newCategory, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        OldCategory = oldCategory;
        NewCategory = newCategory;
        UpdatedAt = updatedAt;
    }
} 