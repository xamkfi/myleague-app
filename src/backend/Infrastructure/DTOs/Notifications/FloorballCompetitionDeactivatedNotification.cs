using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballCompetitionDeactivated notification
    /// </summary>
    public record FloorballCompetitionDeactivatedNotification
    {
        /// <summary>
        /// Gets the ID of the competition
        /// </summary>
        public Guid CompetitionId { get; init; }

        /// <summary>
        /// Gets the name of the competition
        /// </summary>
        public string Name { get; init; } = "Unknown Competition";

        /// <summary>
        /// Gets the date and time when the competition was deactivated
        /// </summary>
        public DateTime DeactivatedOn { get; init; }
    }
}
