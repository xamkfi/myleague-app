using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballPlayerPositionUpdated notification
    /// </summary>
    public record FloorballPlayerPositionUpdatedNotification
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
        /// Gets the position of the player
        /// </summary>
        public string Position { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date and time when the position was updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
} 