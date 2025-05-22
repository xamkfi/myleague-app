using Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.DomainEvents
{
    /// <summary>
    /// Implementation of the domain event dispatcher
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DomainEventDispatcher> _logger;

        /// <summary>
        /// Initializes a new instance of the DomainEventDispatcher class
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="logger">The logger</param>
        public DomainEventDispatcher(
            IServiceProvider serviceProvider,
            ILogger<DomainEventDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Dispatches a domain event to all registered handlers
        /// </summary>
        /// <param name="domainEvent">The domain event to dispatch</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task DispatchAsync(IDomainEvent domainEvent)
        {
            string eventTypeName = domainEvent.GetType().Name;
            _logger.LogInformation("Dispatching domain event {EventType}", eventTypeName);

            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            
            using IServiceScope scope = _serviceProvider.CreateScope();
            IEnumerable<dynamic> handlers = scope.ServiceProvider.GetServices(handlerType).Cast<dynamic>() ?? Array.Empty<dynamic>();

            foreach (dynamic handler in handlers)
            {
                if (handler == null) continue;
                
                try
                {
                    await handler.HandleAsync((dynamic)domainEvent);
                }
                catch (Exception ex)
                {
                    string handlerTypeName = handler.GetType().Name;
                    _logger.LogError(ex, "Error handling domain event {EventType} by handler {HandlerType}", 
                        eventTypeName, handlerTypeName);
                }
            }
        }

        /// <summary>
        /// Dispatches multiple domain events to all registered handlers
        /// </summary>
        /// <param name="domainEvents">The domain events to dispatch</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (IDomainEvent domainEvent in domainEvents)
            {
                await DispatchAsync(domainEvent);
            }
        }
    }
} 
