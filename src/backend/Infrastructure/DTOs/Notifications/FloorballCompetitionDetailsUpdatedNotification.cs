using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballCompetitionDetailsUpdated notification
    /// </summary>
    public record FloorballCompetitionDetailsUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the competition
        /// </summary>
        public Guid CompetitionId { get; init; }

        /// <summary>
        /// Gets the updated name of the competition
        /// </summary>
        public string Name { get; init; } = "Unknown Competition";

        /// <summary>
        /// Gets the date and time when the competition details were updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
}
