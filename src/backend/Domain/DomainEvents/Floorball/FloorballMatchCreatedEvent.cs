using Domain.Entities.Floorball;
using System;

namespace Domain.DomainEvents.Floorball
{
    /// <summary>
    /// Event raised when a floorball match is created
    /// </summary>
    public class FloorballMatchCreatedEvent : FloorballDomainEvent
    {
        public Guid MatchId { get; }
        public Guid SeasonId { get; }
        public Guid HomeTeamId { get; }
        public Guid AwayTeamId { get; }
        public DateTime ScheduledDateTime { get; }
        public string Venue { get; }

        public FloorballMatchCreatedEvent(
            Guid matchId,
            Guid seasonId,
            Guid homeTeamId,
            Guid awayTeamId,
            DateTime scheduledDateTime,
            string venue)
        {
            AggregateId = matchId; // Set the aggregate ID
            MatchId = matchId;
            SeasonId = seasonId;
            HomeTeamId = homeTeamId;
            AwayTeamId = awayTeamId;
            ScheduledDateTime = scheduledDateTime;
            Venue = venue;
        }

        // Private constructor for EF Core/deserialization
        private FloorballMatchCreatedEvent() { }
    }
} 
