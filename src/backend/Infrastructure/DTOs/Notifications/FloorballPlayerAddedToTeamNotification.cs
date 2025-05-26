using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballPlayerAddedToTeam notification
    /// </summary>
    public record FloorballPlayerAddedToTeamNotification
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
        /// Gets the jersey number of the player
        /// </summary>
        public int? JerseyNumber { get; init; }

        /// <summary>
        /// Gets the position of the player
        /// </summary>
        public string Position { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date and time when the player was added to the team
        /// </summary>
        public DateTime AddedOn { get; init; }
    }
} 