using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballCompetitionCompleted notification
    /// </summary>
    public record FloorballCompetitionCompletedNotification
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
        /// Gets the date and time when the competition was completed
        /// </summary>
        public DateTime CompletedOn { get; init; }
    }
}
