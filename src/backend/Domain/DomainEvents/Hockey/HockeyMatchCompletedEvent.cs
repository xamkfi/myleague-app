

namespace Domain.DomainEvents.Hockey
{
    /// <summary>
    /// Event raised when a hockey match is completed
    /// </summary>
    public class HockeyMatchCompletedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the date and time when the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; }

        /// <summary>
        /// Gets the final home score
        /// </summary>
        public int HomeScore { get; }

        /// <summary>
        /// Gets the final away score
        /// </summary>
        public int AwayScore { get; }

        /// <summary>
        /// Gets whether the match went to overtime
        /// </summary>
        public bool WentToOvertime { get; }

        /// <summary>
        /// Gets whether the match went to shootout
        /// </summary>
        public bool WentToShootout { get; }

        /// <summary>
        /// Initializes a new instance of the HockeyMatchCompletedEvent class
        /// </summary>
        /// <param name="matchId">The ID of the match</param>
        /// <param name="homeScore">The final home score</param>
        /// <param name="awayScore">The final away score</param>
        /// <param name="wentToOvertime">Whether the match went to overtime</param>
        /// <param name="wentToShootout">Whether the match went to shootout</param>
        public HockeyMatchCompletedEvent(
            Guid matchId,
            int homeScore,
            int awayScore,
            bool wentToOvertime,
            bool wentToShootout)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            MatchId = matchId;
            HomeScore = homeScore;
            AwayScore = awayScore;
            WentToOvertime = wentToOvertime;
            WentToShootout = wentToShootout;
        }
    }
}
