using System;
using Domain.DomainEvents.Floorball;

namespace Domain.DomainEvents.Floorball
{
    /// <summary>
    /// Domain event for a save in a floorball match
    /// </summary>
    public class FloorballSaveEvent : FloorballDomainEvent
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; }

        /// <summary>
        /// Gets the ID of the team whose goalie made the save
        /// </summary>
        public Guid TeamId { get; }

        /// <summary>
        /// Gets the ID of the goalie who made the save
        /// </summary>
        public Guid GoalieId { get; }

        /// <summary>
        /// Gets the period number when the save was made
        /// </summary>
        public int PeriodNumber { get; }

        /// <summary>
        /// Gets the time in seconds when the save was made in the period
        /// </summary>
        public int TimeInSeconds { get; }

        /// <summary>
        /// Gets whether the save was made in overtime
        /// </summary>
        public bool WasInOvertime { get; }

        /// <summary>
        /// Gets whether the save was made in shootout
        /// </summary>
        public bool WasInShootout { get; }

        /// <summary>
        /// Initializes a new instance of the FloorballSaveEvent class
        /// </summary>
        /// <param name="matchId">The ID of the match</param>
        /// <param name="teamId">The ID of the team whose goalie made the save</param>
        /// <param name="goalieId">The ID of the goalie who made the save</param>
        /// <param name="periodNumber">The period number when the save was made</param>
        /// <param name="timeInSeconds">The time in seconds when the save was made in the period</param>
        /// <param name="wasInOvertime">Whether the save was made in overtime</param>
        /// <param name="wasInShootout">Whether the save was made in shootout</param>
        public FloorballSaveEvent(
            Guid matchId,
            Guid teamId,
            Guid goalieId,
            int periodNumber,
            int timeInSeconds,
            bool wasInOvertime = false,
            bool wasInShootout = false)
        {
            if (periodNumber < 1)
                throw new ArgumentException("Period number must be positive", nameof(periodNumber));
            if (timeInSeconds < 0 || timeInSeconds > 1200)
                throw new ArgumentException("Time must be between 0 and 1200 seconds", nameof(timeInSeconds));

            MatchId = matchId;
            TeamId = teamId;
            GoalieId = goalieId;
            PeriodNumber = periodNumber;
            TimeInSeconds = timeInSeconds;
            WasInOvertime = wasInOvertime;
            WasInShootout = wasInShootout;
        }
        /// <summary>
        /// Parameterless constructor for serialization and event replay
        /// </summary>
        private FloorballSaveEvent() { }
    }
}
