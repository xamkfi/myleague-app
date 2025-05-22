using Domain.DomainEvents;

namespace MyLeague.Infrastructure.DomainEvents
{
    /// <summary>
    /// Interface for domain event handlers
    /// </summary>
    /// <typeparam name="TEvent">The type of event to handle</typeparam>
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// Handles the specified domain event
        /// </summary>
        /// <param name="domainEvent">The domain event to handle</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task HandleAsync(TEvent domainEvent);
    }
} 