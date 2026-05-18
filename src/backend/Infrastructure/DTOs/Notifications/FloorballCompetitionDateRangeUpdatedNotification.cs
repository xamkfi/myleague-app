using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballCompetitionDateRangeUpdated notification
    /// </summary>
    public record FloorballCompetitionDateRangeUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the competition
        /// </summary>
        public Guid CompetitionId { get; init; }

        /// <summary>
        /// Gets the updated start date of the competition
        /// </summary>
        public DateTime StartDate { get; init; }

        /// <summary>
        /// Gets the updated end date of the competition
        /// </summary>
        public DateTime EndDate { get; init; }

        /// <summary>
        /// Gets the date and time when the competition date range was updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
}
