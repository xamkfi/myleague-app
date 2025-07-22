using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

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
        /// Subscribes a client to a specific event type
        /// </summary>
        /// <param name="connectionId">The connection ID</param>
        /// <param name="eventType">The event type to subscribe to</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SubscribeToEventTypeAsync(string connectionId, string eventType)
        {
            await _notifier.SubscribeToEventTypeAsync(connectionId, eventType);
            _logger.LogInformation("Client {ConnectionId} subscribed to event type {EventType}", connectionId, eventType);
        }

        /// <summary>
        /// Unsubscribes a client from a specific event type
        /// </summary>
        /// <param name="connectionId">The connection ID</param>
        /// <param name="eventType">The event type to unsubscribe from</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task UnsubscribeFromEventTypeAsync(string connectionId, string eventType)
        {
            await _notifier.UnsubscribeFromEventTypeAsync(connectionId, eventType);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from event type {EventType}", connectionId, eventType);
        }
    }
} 