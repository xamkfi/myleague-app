using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSeasonDetailsUpdated notification
    /// </summary>
    public record FloorballSeasonDetailsUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; init; }

        /// <summary>
        /// Gets the updated name of the season
        /// </summary>
        public string Name { get; init; } = "Unknown Season";

        /// <summary>
        /// Gets the date and time when the season details were updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
} 