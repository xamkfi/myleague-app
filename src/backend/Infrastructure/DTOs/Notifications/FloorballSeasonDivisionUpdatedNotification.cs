using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSeasonDivisionUpdated notification
    /// </summary>
    public record FloorballSeasonDivisionUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the season
        /// </summary>
        public Guid SeasonId { get; init; }

        /// <summary>
        /// Gets the updated division of the season
        /// </summary>
        public string Division { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date and time when the season division was updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
} 