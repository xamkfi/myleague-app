namespace Domain.ValueObjects.Hockey
{
    /// <summary>
    /// Represents the score for a single period in a hockey match
    /// </summary>
    public class HockeyPeriodScore
    {
        /// <summary>
        /// Gets the period number
        /// </summary>
        public int PeriodNumber { get; private set; }

        /// <summary>
        /// Gets the home team's score for this period
        /// </summary>
        public int HomeScore { get; private set; }

        /// <summary>
        /// Gets the away team's score for this period
        /// </summary>
        public int AwayScore { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private HockeyPeriodScore()
        {
        }

        /// <summary>
        /// Initializes a new instance of the HockeyPeriodScore class
        /// </summary>
        /// <param name="periodNumber">The period number</param>
        /// <param name="homeScore">the home team's score</param>
        /// <param name="awayScore">The away team's score</param>
        public HockeyPeriodScore(int periodNumber, int homeScore, int awayScore)
        {
            if (periodNumber <= 0)
                throw new ArgumentException("Period number must be positive.", nameof(periodNumber));
            if (homeScore < 0)
                throw new ArgumentException("Home score cannot be negative.", nameof(homeScore));
            if (awayScore < 0)
                throw new ArgumentException("Away score cannot be negative.", nameof(awayScore));

            PeriodNumber = periodNumber;
            HomeScore = homeScore;
            AwayScore = awayScore;
        }

        /// <summary>
        /// Updates the home team's score
        /// </summary>
        /// <param name="score">The new score</param>
        public void UpdateHomeScore(int score)
        {
            if (score < 0)
                throw new ArgumentException("Score cannot be negative.",nameof(score));

            HomeScore = score;
        }

        /// <summary>
        /// Update the away team's score
        /// </summary>
        /// <param name="score">The new score</param>
        public void UpdateAwayScore(int score)
        {
            if (score < 0)
                throw new ArgumentException("Score cannot be negative.", nameof(score));

            AwayScore = score;
        }

        /// <summary>
        /// Increments the home team's score by 1
        /// </summary>
        public void IncrementHomeScore()
        {
            HomeScore++;
        }

        /// <summary>
        /// Increments the away team's score by 1
        /// </summary>
        public void IncrementAwayScore()
        {
            AwayScore++;
        }
    }
}
