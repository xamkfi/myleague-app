namespace Application.Interfaces.Common
{
    /// <summary>
    /// Marker interface for notification payloads that target a specific match.
    /// Implementations are automatically routed to the match's SignalR group.
    /// </summary>
    public interface IMatchNotification
    {
        Guid MatchId { get; }
    }

    public record MatchNotificationPayload(Guid MatchId) : IMatchNotification;
}
