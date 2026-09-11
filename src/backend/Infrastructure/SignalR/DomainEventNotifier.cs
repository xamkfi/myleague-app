using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyLeague.Infrastructure.SignalR
{
    public class DomainEventNotifier
    {
        private readonly IHubContext<DomainEventHub> _hubContext;
        private readonly ILogger<DomainEventNotifier> _logger;

        public DomainEventNotifier(
            IHubContext<DomainEventHub> hubContext,
            ILogger<DomainEventNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Sends an event to the event-type group only (clients who subscribed to this specific event type).
        /// Does NOT broadcast to all clients.
        /// </summary>
        public async Task NotifyEventGroupAsync(string eventName, string payloadJson)
        {
            try
            {
                await _hubContext.Clients.Group(eventName).SendAsync("DomainEvent", eventName, payloadJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying event group {EventName}", eventName);
            }
        }

        /// <summary>
        /// Sends an event to a specific match group (clients who subscribed to this match).
        /// </summary>
        public async Task NotifyMatchGroupAsync(Guid matchId, string eventName, string payloadJson)
        {
            try
            {
                string groupName = $"Match_{matchId}";
                await _hubContext.Clients.Group(groupName).SendAsync("MatchEvent", eventName, payloadJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying match {MatchId} clients of event {EventName}", matchId, eventName);
            }
        }

        public async Task SubscribeToEventTypeAsync(string connectionId, string eventType)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, eventType);
            _logger.LogInformation("Client {ConnectionId} subscribed to event type {EventType}", connectionId, eventType);
        }

        public async Task UnsubscribeFromEventTypeAsync(string connectionId, string eventType)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, eventType);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from event type {EventType}", connectionId, eventType);
        }

        public async Task SubscribeToMatchAsync(string connectionId, Guid matchId)
        {
            string groupName = $"Match_{matchId}";
            await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} subscribed to match {MatchId}", connectionId, matchId);
        }

        public async Task UnsubscribeFromMatchAsync(string connectionId, Guid matchId)
        {
            string groupName = $"Match_{matchId}";
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from match {MatchId}", connectionId, matchId);
        }
    }
} 
