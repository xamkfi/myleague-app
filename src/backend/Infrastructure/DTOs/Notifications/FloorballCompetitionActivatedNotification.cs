using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballCompetitionActivated notification
    /// </summary>
    public record FloorballCompetitionActivatedNotification
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
        /// Gets the date and time when the competition was activated
        /// </summary>
        public DateTime ActivatedOn { get; init; }
    }
}
