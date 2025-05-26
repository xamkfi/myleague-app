using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballPenaltyAssigned notification
    /// </summary>
    public record FloorballPenaltyAssignedNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets the time when the event occurred
        /// </summary>
        public DateTime EventTime { get; init; }

        /// <summary>
        /// Gets the type of penalty
        /// </summary>
        public string PenaltyType { get; init; } = string.Empty;

        /// <summary>
        /// Gets the ID of the team
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the ID of the player
        /// </summary>
        public Guid PlayerId { get; init; }

        /// <summary>
        /// Gets information about the home team
        /// </summary>
        public TeamInfo HomeTeam { get; init; } = new();

        /// <summary>
        /// Gets information about the away team
        /// </summary>
        public TeamInfo AwayTeam { get; init; } = new();

        /// <summary>
        /// Simple team information
        /// </summary>
        public record TeamInfo
        {
            /// <summary>
            /// Gets the ID of the team
            /// </summary>
            public Guid? Id { get; init; }

            /// <summary>
            /// Gets the name of the team
            /// </summary>
            public string Name { get; init; } = "Unknown";
        }
    }
} 