using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSeasonCompleted notification
    /// </summary>
    public record FloorballSeasonCompletedNotification
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
        /// Gets the date and time when the season was completed
        /// </summary>
        public DateTime CompletedOn { get; init; }
    }
} 