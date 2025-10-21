using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballGoalieSave notification
    /// </summary>
    public record FloorballSaveNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets the ID of the team whose goalie made the save
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the ID of the goalie who made the save
        /// </summary>
        public Guid GoalieId { get; init; }

        /// <summary>
        /// Gets the period number when the save was made
        /// </summary>
        public int PeriodNumber { get; init; }

        /// <summary>
        /// Gets the time in seconds when the save was made in the period
        /// </summary>
        public int TimeInSeconds { get; init; }

        /// <summary>
        /// Gets the time when the save was made
        /// </summary>
        public DateTime EventTime { get; init; }

        /// <summary>
        /// Gets whether the save was made in overtime
        /// </summary>
        public bool WasInOvertime { get; init; }

        /// <summary>
        /// Gets whether the save was made in shootout
        /// </summary>
        public bool WasInShootout { get; init; }

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
