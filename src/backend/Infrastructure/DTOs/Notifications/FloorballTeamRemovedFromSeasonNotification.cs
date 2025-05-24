using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballTeamRemovedFromSeason notification
    /// </summary>
    public record FloorballTeamRemovedFromSeasonNotification
    {
        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; init; }

        /// <summary>
        /// Gets the name of the season
        /// </summary>
        public string SeasonName { get; init; } = "Unknown Season";

        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the name of the team
        /// </summary>
        public string TeamName { get; init; } = "Unknown Team";

        /// <summary>
        /// Gets the date and time when the team was removed from the season
        /// </summary>
        public DateTime RemovedOn { get; init; }
    }
} 