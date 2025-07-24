using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.SignalR
{
    /// <summary>
    /// SignalR hub for domain events
    /// </summary>
    public class DomainEventHub : Hub
    {
        private readonly DomainEventNotifier _notifier;
        private readonly ILogger<DomainEventHub> _logger;

        /// <summary>
        /// Initializes a new instance of the DomainEventHub class
        /// </summary>
        public DomainEventHub(DomainEventNotifier notifier, ILogger<DomainEventHub> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        /// <summary>
        /// Connection ID for the current connection
        /// </summary>
        /// <returns>The connection ID</returns>
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        /// <summary>
        /// Subscribes the current connection to a specific event type group using the notifier
        /// </summary>
        /// <param name="eventType">The event type to subscribe to</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SubscribeToEventTypeAsync(string eventType)
        {
            await _notifier.SubscribeToEventTypeAsync(Context.ConnectionId, eventType);
            _logger.LogInformation("Client {ConnectionId} subscribed to event type {EventType}", Context.ConnectionId, eventType);
        }

        /// <summary>
        /// Unsubscribes the current connection from a specific event type group using the notifier
        /// </summary>
        /// <param name="eventType">The event type to unsubscribe from</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UnsubscribeFromEventTypeAsync(string eventType)
        {
            await _notifier.UnsubscribeFromEventTypeAsync(Context.ConnectionId, eventType);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from event type {EventType}", Context.ConnectionId, eventType);
        }

        /// <summary>
        /// Subscribes the current connection to a specific match group
        /// </summary>
        /// <param name="matchId">The match ID to subscribe to</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SubscribeToMatchAsync(Guid matchId)
        {
            string groupName = $"Match_{matchId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Unsubscribes the current connection from a specific match group
        /// </summary>
        /// <param name="matchId">The match ID to unsubscribe from</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UnsubscribeFromMatchAsync(Guid matchId)
        {
            string groupName = $"Match_{matchId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
