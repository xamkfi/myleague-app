using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSeasonDateRangeUpdated notification
    /// </summary>
    public record FloorballSeasonDateRangeUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; init; }

        /// <summary>
        /// Gets the updated start date of the season
        /// </summary>
        public DateTime StartDate { get; init; }

        /// <summary>
        /// Gets the updated end date of the season
        /// </summary>
        public DateTime EndDate { get; init; }

        /// <summary>
        /// Gets the date and time when the season date range was updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
} 