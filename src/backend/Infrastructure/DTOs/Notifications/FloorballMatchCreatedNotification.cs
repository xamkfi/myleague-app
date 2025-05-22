using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballMatchCreated notification
    /// </summary>
    public record FloorballMatchCreatedNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets the scheduled date and time of the match
        /// </summary>
        public DateTime ScheduledDateTime { get; init; }

        /// <summary>
        /// Gets the location of the match
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets information about the home team
        /// </summary>
        public TeamInfo HomeTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets information about the away team
        /// </summary>
        public TeamInfo AwayTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets the date and time when the match was created
        /// </summary>
        public DateTime CreatedOn { get; init; }
    }
} 