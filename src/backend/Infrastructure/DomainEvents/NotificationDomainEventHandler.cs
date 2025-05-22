using Domain.DomainEvents;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.SignalR;
using System.Threading;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.DomainEvents
{
    /// <summary>
    /// Base class for domain event handlers that build and send notifications
    /// </summary>
    /// <typeparam name="TEvent">The type of domain event to handle</typeparam>
    public abstract class NotificationDomainEventHandler<TEvent> : IDomainEventHandler<TEvent> 
        where TEvent : IDomainEvent
    {
        private readonly INotificationSender _notificationSender;
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the NotificationDomainEventHandler class
        /// </summary>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        protected NotificationDomainEventHandler(
            INotificationSender notificationSender,
            ILogger logger)
        {
            _notificationSender = notificationSender;
            _logger = logger;
        }

        /// <summary>
        /// Handles the domain event
        /// </summary>
        /// <param name="domainEvent">The domain event to handle</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public virtual async Task HandleAsync(TEvent domainEvent)
        {
            _logger.LogInformation("Handling domain event {EventType}", typeof(TEvent).Name);
            
            (string eventName, object? notification) = await BuildNotificationAsync(domainEvent, CancellationToken.None);
            
            if (notification != null)
            {
                await _notificationSender.SendNotificationAsync(eventName, notification);
            }
        }

        /// <summary>
        /// Builds the notification payload from the domain event
        /// </summary>
        /// <param name="domainEvent">The domain event</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A tuple containing the event name and notification payload</returns>
        protected abstract Task<(string EventName, object? Notification)> BuildNotificationAsync(
            TEvent domainEvent, 
            CancellationToken cancellationToken = default);
    }
} 
