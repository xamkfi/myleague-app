using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballPlayerRegistered notification
    /// </summary>
    public record FloorballPlayerRegisteredNotification
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
        /// Gets the ID of the person associated with the player
        /// </summary>
        public Guid PersonId { get; init; }

        /// <summary>
        /// Gets the date and time of registration
        /// </summary>
        public DateTime RegistrationTime { get; init; }
    }
} 