using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballGoalScored notification
    /// </summary>
    public record FloorballGoalScoredNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets the ID of the team that scored the goal
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the ID of the player who scored the goal
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// Gets the period number when the goal was scored
        /// </summary>
        public int PeriodNumber { get; init; }

        /// <summary>
        /// Gets the time when the goal was scored
        /// </summary>
        public DateTime EventTime { get; init; }

        /// <summary>
        /// Gets information about the home team
        /// </summary>
        public TeamInfo HomeTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets information about the away team
        /// </summary>
        public TeamInfo AwayTeam { get; init; } = new TeamInfo();
    }
}  