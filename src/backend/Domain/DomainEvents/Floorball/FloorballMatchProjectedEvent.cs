using System;
using Domain.DomainEvents;

namespace Domain.DomainEvents.Floorball
{
    /// <summary>
    /// This event is raised after a new FloorballMatch has been
    /// successfully projected to the read model.
    /// </summary>
    public sealed class FloorballMatchProjectedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }

        public FloorballMatchProjectedEvent(Guid matchId)
        {
            Id = matchId;
            OccurredOn = DateTime.UtcNow;
        }
    }
} 
