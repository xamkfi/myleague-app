using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballPlayerStatUpdated notification
    /// </summary>
    public record FloorballPlayerStatUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the player
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// Gets the name of the player
        /// </summary>
        public string PlayerName { get; init; } = "Unknown";

        /// <summary>
        /// Gets the date and time when the player stats were updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
} 