using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSeasonDeactivated notification
    /// </summary>
    public record FloorballSeasonDeactivatedNotification
    {
        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; init; }

        /// <summary>
        /// Gets the name of the season
        /// </summary>
        public string Name { get; init; } = "Unknown Season";

        /// <summary>
        /// Gets the date and time when the season was deactivated
        /// </summary>
        public DateTime DeactivatedOn { get; init; }
    }
} 