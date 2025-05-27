using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects.Hockey
{
    /// <summary>
    /// Represents the score of a period in a hockey match
    /// </summary>
    public class PeriodScoreValue
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get;private set; }

        /// <summary>
        /// Gets the period number
        /// </summary>
        public int PeriodNumber { get; private set; }

        /// <summary>
        /// Gets the ID of the home team
        /// </summary>
        public Guid HomeTeamId { get; private set; }

        /// <summary>
        /// Gets the ID of the away team
        /// </summary>
        public Guid AwayTeamId { get; private set; }

        /// <summary>
        /// Gets the home team's score
        /// </summary>
        public int HomeScore { get; private set; }

        /// <summary>
        /// Gets the away team's score
        /// </summary>
        public int AwayScore { get; private set; }

        /// <summary>
        /// Gets whether the period is completed
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private PeriodScoreValue() {}

        /// <summary>
        /// Initializes a new instance of the PeriodScoreValue class
        /// </summary>
        /// <param name="matchId">The ID of the match</param>
        /// <param name="periodNumber">The period number</param>
        /// <param name="homeTeamId">The ID of the home team</param>
        /// <param name="awayTeamId">The ID of the away team</param>
        public PeriodScoreValue(Guid matchId, int periodNumber, Guid homeTeamId, Guid awayTeamId)
        {
            if(periodNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period number must be positive.");
            MatchId = matchId;
            PeriodNumber = periodNumber;
            HomeTeamId = homeTeamId;
            AwayTeamId = awayTeamId;
            HomeScore = 0;
            IsCompleted = false;
        }

        /// <summary>
        /// Updates the score
        /// </summary>
        /// <param name="homeScore"></param>
        /// <param name="awayScore"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void UpdateScore(int homeScore, int awayScore)
        {
            if (homeScore < 0)
                throw new ArgumentOutOfRangeException(nameof(homeScore), "Score cannot be negative.");
            if (awayScore < 0)
                throw new ArgumentOutOfRangeException(nameof(awayScore), "Score cannot be negative.");

            HomeScore = homeScore;
            AwayScore = awayScore;
        }

        /// <summary>
        /// Completes the period
        /// </summary>
        public void Complete()
        {
            IsCompleted = true;
        }
    }
}
