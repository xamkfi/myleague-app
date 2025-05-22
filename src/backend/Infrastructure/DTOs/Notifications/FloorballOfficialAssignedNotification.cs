using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballOfficialAssigned notification
    /// </summary>
    public record FloorballOfficialAssignedNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets the ID of the referee
        /// </summary>
        public Guid RefereeId { get; init; }

        /// <summary>
        /// Gets the name of the official
        /// </summary>
        public string OfficialName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the role of the official
        /// </summary>
        public string Role { get; init; } = string.Empty;

        /// <summary>
        /// Gets information about the home team
        /// </summary>
        public TeamInfo HomeTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets information about the away team
        /// </summary>
        public TeamInfo AwayTeam { get; init; } = new TeamInfo();

        /// <summary>
        /// Gets the scheduled date and time of the match
        /// </summary>
        public DateTime ScheduledDateTime { get; init; }

        /// <summary>
        /// Gets the date and time when the official was assigned
        /// </summary>
        public DateTime AssignedOn { get; init; }
    }
} 