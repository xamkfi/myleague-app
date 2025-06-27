using System;
using Domain.Entities.Floorball;

namespace Domain.DomainEvents.Floorball
{
    /// <summary>
    /// Base class for all floorball domain events
    /// </summary>
    public class FloorballDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the identifier of the aggregate this event belongs to
        /// </summary>
        public Guid AggregateId { get; protected set; }

        /// <summary>
        /// Gets the unique identifier for this event instance
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Gets the date and time when this event occurred
        /// </summary>
        public DateTime OccurredOn { get; protected set; }

        /// <summary>
        /// Gets the event type name
        /// </summary>
        public string EventType { get; protected set; }

        /// <summary>
        /// To store serialized event data
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected FloorballDomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EventType = GetType().Name;
            Data = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorballDomainEvent"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        protected FloorballDomainEvent(Guid aggregateId) : this()
        {
            AggregateId = aggregateId;
        }
    }
} 
