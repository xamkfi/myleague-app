using System;

namespace Domain.DomainEvents.Common
{
    /// <summary>
    /// Base class for all common domain events
    /// </summary>
    public abstract class CommonDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier for this event instance
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the date and time when this event occurred
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the event type name
        /// </summary>
        public string EventType => GetType().Name;
    }
} 