using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballCompetitionDivisionUpdated notification
    /// </summary>
    public record FloorballCompetitionDivisionUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the competition
        /// </summary>
        public Guid CompetitionId { get; init; }

        /// <summary>
        /// Gets the updated division ID of the competition
        /// </summary>
        public Guid DivisionId { get; init; }

        /// <summary>
        /// Gets the date and time when the competition division was updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
}
