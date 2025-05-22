using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballMatchAddedToSeason notification
    /// </summary>
    public record FloorballMatchAddedToSeasonNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; init; }

        /// <summary>
        /// Gets the name of the season
        /// </summary>
        public string SeasonName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the scheduled date and time of the match
        /// </summary>
        public DateTime ScheduledDateTime { get; init; }

        /// <summary>
        /// Gets information about the home team
        /// </summary>
        public TeamInfo HomeTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets information about the away team
        /// </summary>
        public TeamInfo AwayTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets the date and time when the match was added to the season
        /// </summary>
        public DateTime AddedOn { get; init; }
    }
} 