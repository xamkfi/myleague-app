using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballTeamRemoved notification
    /// </summary>
    public record FloorballTeamRemovedNotification
    {
        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the date and time when the team was removed
        /// </summary>
        public DateTime RemovedOn { get; init; }
    }
} 