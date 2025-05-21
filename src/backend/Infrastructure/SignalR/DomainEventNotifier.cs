using Domain.DomainEvents;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyLeague.Infrastructure.SignalR
{
    /// <summary>
    /// Service that notifies clients of domain events using SignalR
    /// </summary>
    public class DomainEventNotifier
    {
        private readonly IHubContext<DomainEventHub> _hubContext;
        private readonly ILogger<DomainEventNotifier> _logger;

        /// <summary>
        /// Initializes a new instance of the DomainEventNotifier class
        /// </summary>
        /// <param name="hubContext">The hub context</param>
        /// <param name="logger">The logger</param>
        public DomainEventNotifier(
            IHubContext<DomainEventHub> hubContext,
            ILogger<DomainEventNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Notifies clients of a domain event
        /// </summary>
        /// <param name="domainEvent">The domain event</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task NotifyAsync(IDomainEvent domainEvent)
        {
            try
            {
                string eventType = domainEvent.GetType().Name;
                string eventJson = JsonSerializer.Serialize(domainEvent);
                
                _logger.LogInformation("Notifying clients of domain event {EventType}", eventType);
                
                // Notify all clients of the event
                await _hubContext.Clients.All.SendAsync("DomainEvent", eventType, eventJson);
                
                // Notify clients in the group for this specific event type
                await _hubContext.Clients.Group(eventType).SendAsync("DomainEvent", eventType, eventJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying clients of domain event {EventType}", domainEvent.GetType().Name);
            }
        }

        /// <summary>
        /// Subscribes a client to a specific event type
        /// </summary>
        /// <param name="connectionId">The connection ID</param>
        /// <param name="eventType">The event type to subscribe to</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SubscribeToEventTypeAsync(string connectionId, string eventType)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, eventType);
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
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, eventType);
            _logger.LogInformation("Client {ConnectionId} unsubscribed from event type {EventType}", connectionId, eventType);
        }
    }
} 
