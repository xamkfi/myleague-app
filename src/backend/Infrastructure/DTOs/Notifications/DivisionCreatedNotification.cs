using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the DivisionCreated notification
    /// </summary>
    public record DivisionCreatedNotification
    {
        /// <summary>
        /// Gets the ID of the division
        /// </summary>
        public Guid DivisionId { get; init; }

        /// <summary>
        /// Gets the name of the division
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the description of the division
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets the level of the division
        /// </summary>
        public int Level { get; init; }

        /// <summary>
        /// Gets the sport type of the division
        /// </summary>
        public string SportType { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date and time when the division was created
        /// </summary>
        public DateTime CreatedOn { get; init; }
    }
} 