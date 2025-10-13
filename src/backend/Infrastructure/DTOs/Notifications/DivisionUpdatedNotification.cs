using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the DivisionUpdated notification
    /// </summary>
    public record DivisionUpdatedNotification
    {
        /// <summary>
        /// Gets the ID of the division
        /// </summary>
        public Guid DivisionId { get; init; }

        /// <summary>
        /// Gets the updated name of the division
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated description of the division
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated level of the division
        /// </summary>
        public int Level { get; init; }

        /// <summary>
        /// Gets the date and time when the division was updated
        /// </summary>
        public DateTime UpdatedOn { get; init; }
    }
} 