using System;
using Domain.Enums.Common;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the DivisionDeactivated notification
    /// </summary>
    public record DivisionDeactivatedNotification
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
        /// Gets the sport type of the division
        /// </summary>
        public SportsCategory SportType { get; init; } = SportsCategory.None;

        /// <summary>
        /// Gets the date and time when the division was deactivated
        /// </summary>
        public DateTime DeactivatedOn { get; init; }
    }
} 