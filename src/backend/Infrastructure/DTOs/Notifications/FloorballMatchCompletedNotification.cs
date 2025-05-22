using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballMatchCompleted notification
    /// </summary>
    public record FloorballMatchCompletedNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets information about the home team
        /// </summary>
        public TeamInfo HomeTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets information about the away team
        /// </summary>
        public TeamInfo AwayTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets the final score of the home team
        /// </summary>
        public int HomeScore { get; init; }

        /// <summary>
        /// Gets the final score of the away team
        /// </summary>
        public int AwayScore { get; init; }

        /// <summary>
        /// Gets whether the match went to overtime
        /// </summary>
        public bool WentToOvertime { get; init; }

        /// <summary>
        /// Gets whether the match went to shootout
        /// </summary>
        public bool WentToShootout { get; init; }

        /// <summary>
        /// Gets the date and time when the match was completed
        /// </summary>
        public DateTime CompletedOn { get; init; }
    }
} 