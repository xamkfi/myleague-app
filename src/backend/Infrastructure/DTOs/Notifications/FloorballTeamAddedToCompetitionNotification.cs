using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballTeamAddedToCompetition notification
    /// </summary>
    public record FloorballTeamAddedToCompetitionNotification
    {
        /// <summary>
        /// Gets the ID of the competition
        /// </summary>
        public Guid CompetitionId { get; init; }

        /// <summary>
        /// Gets the name of the competition
        /// </summary>
        public string CompetitionName { get; init; } = "Unknown Competition";

        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the name of the team
        /// </summary>
        public string TeamName { get; init; } = "Unknown Team";

        /// <summary>
        /// Gets the date and time when the team was added to the competition
        /// </summary>
        public DateTime AddedOn { get; init; }
    }
}
