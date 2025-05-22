using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSeasonActivated notification
    /// </summary>
    public record FloorballSeasonActivatedNotification
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
        /// Gets the date and time when the season was activated
        /// </summary>
        public DateTime ActivatedOn { get; init; }
    }
} 