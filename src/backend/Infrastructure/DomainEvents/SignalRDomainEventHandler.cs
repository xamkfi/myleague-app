using Domain.DomainEvents;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents
{
    /// <summary>
    /// Base class for domain event handlers that notify clients via SignalR
    /// </summary>
    /// <typeparam name="TEvent">The type of domain event to handle</typeparam>
    public abstract class SignalRDomainEventHandler<TEvent> : IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        private readonly DomainEventNotifier _notifier;
        private readonly ILogger<SignalRDomainEventHandler<TEvent>> _logger;

        /// <summary>
        /// Initializes a new instance of the SignalRDomainEventHandler class
        /// </summary>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        protected SignalRDomainEventHandler(
            DomainEventNotifier notifier,
            ILogger<SignalRDomainEventHandler<TEvent>> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        /// <summary>
        /// Handles the domain event
        /// </summary>
        /// <param name="domainEvent">The domain event to handle</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public virtual async Task HandleAsync(TEvent domainEvent)
        {
            _logger.LogInformation("Handling domain event {EventType} with SignalR notification", typeof(TEvent).Name);
            
            await ProcessEventAsync(domainEvent);
            await _notifier.NotifyAsync(domainEvent);
        }

        /// <summary>
        /// Processes the domain event before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected virtual Task ProcessEventAsync(TEvent domainEvent)
        {
            // Default implementation does no additional processing
            return Task.CompletedTask;
        }
    }
} 