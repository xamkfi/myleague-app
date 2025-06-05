using Domain.DomainEvents;

namespace Domain.DomainEvents.Common;

/// <summary>
/// Event raised when a news article's content is updated
/// </summary>
    public class NewsArticleContentUpdatedEvent : IDomainEvent
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
    /// Gets the previous title
    /// </summary>
    public string OldTitle { get; }

    /// <summary>
    /// Gets the new title
    /// </summary>
    public string NewTitle { get; }

    /// <summary>
    /// Gets the previous content
    /// </summary>
    public string OldContent { get; }

    /// <summary>
    /// Gets the new content
    /// </summary>
    public string NewContent { get; }

    /// <summary>
    /// Gets the previous summary
    /// </summary>
    public string? OldSummary { get; }

    /// <summary>
    /// Gets the new summary
    /// </summary>
    public string? NewSummary { get; }

    /// <summary>
    /// Gets the date and time when the content was updated
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the NewsContentUpdatedEvent class
    /// </summary>
    /// <param name="newsId">The ID of the news article</param>
    /// <param name="oldTitle">The previous title</param>
    /// <param name="newTitle">The new title</param>
    /// <param name="oldContent">The previous content</param>
    /// <param name="newContent">The new content</param>
    /// <param name="oldSummary">The previous summary</param>
    /// <param name="newSummary">The new summary</param>
    /// <param name="updatedAt">The date and time when the content was updated</param>
    public NewsArticleContentUpdatedEvent(Guid newsId, string oldTitle, string newTitle, string oldContent, string newContent, string? oldSummary, string? newSummary, DateTime updatedAt)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        NewsId = newsId;
        OldTitle = oldTitle;
        NewTitle = newTitle;
        OldContent = oldContent;
        NewContent = newContent;
        OldSummary = oldSummary;
        NewSummary = newSummary;
        UpdatedAt = updatedAt;
    }
}
