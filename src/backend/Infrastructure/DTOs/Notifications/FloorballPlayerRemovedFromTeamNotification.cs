using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballPlayerRemovedFromTeam notification
    /// </summary>
    public record FloorballPlayerRemovedFromTeamNotification
    {
        /// <summary>
        /// Gets the ID of the player
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the name of the player
        /// </summary>
        public string PlayerName { get; init; } = "Unknown";

        /// <summary>
        /// Gets the name of the team
        /// </summary>
        public string TeamName { get; init; } = "Unknown Team";

        /// <summary>
        /// Gets the date and time when the player was removed from the team
        /// </summary>
        public DateTime RemovedOn { get; init; }
    }
} 