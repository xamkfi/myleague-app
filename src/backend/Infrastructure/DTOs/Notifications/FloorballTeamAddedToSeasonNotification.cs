using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballTeamAddedToSeason notification
    /// </summary>
    public record FloorballTeamAddedToSeasonNotification
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
        /// Gets the date and time when the team was added to the season
        /// </summary>
        public DateTime AddedOn { get; init; }
    }
} 