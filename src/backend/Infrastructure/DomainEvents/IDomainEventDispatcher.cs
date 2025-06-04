using Domain.DomainEvents;

namespace MyLeague.Infrastructure.DomainEvents
{
    /// <summary>
    /// Interface for domain event dispatchers
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches a domain event to all registered handlers
        /// </summary>
        /// <param name="domainEvent">The domain event to dispatch</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DispatchAsync(IDomainEvent domainEvent);
        
        /// <summary>
        /// Dispatches multiple domain events to all registered handlers
        /// </summary>
        /// <param name="domainEvents">The domain events to dispatch</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents);
    }
} 