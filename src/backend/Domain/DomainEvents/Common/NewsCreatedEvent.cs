using Domain.DomainEvents;
using Domain.ValueObjects.Common;

public record NewsCreatedEvent(NewsId NewsId, string Title, string? Author, DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
public record NewsContentUpdatedEvent(NewsId NewsId, string OldTitle, string NewTitle, string OldContent, string NewContent, string? OldSummary, string? NewSummary, DateTime UpdatedAt) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
// Repeat similarly for:
// NewsImageUpdatedEvent
// NewsCategoryChangedEvent
// NewsTagAddedEvent
// NewsTagRemovedEvent
// NewsArchivedEvent
// NewsRestoredEvent
