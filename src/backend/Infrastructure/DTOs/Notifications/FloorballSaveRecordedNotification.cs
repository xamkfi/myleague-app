using System;

namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSaveRecorded notification
    /// </summary>
    public record FloorballSaveRecordedNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; init; }

        /// <summary>
        /// Gets the ID of the team whose goalie made the save
        /// </summary>
        public Guid TeamId { get; init; }

        /// <summary>
        /// Gets the ID of the goalie who made the save
        /// </summary>
        public Guid GoalieId { get; init; }

        /// <summary>
        /// Gets the period number when the save was recorded
        /// </summary>
        public int PeriodNumber { get; init; }

        /// <summary>
        /// Gets the time in seconds when the save was recorded in the period
        /// </summary>
        public int TimeInSeconds { get; init; }

        /// <summary>
        /// Gets whether the save was recorded in overtime
        /// </summary>
        public bool IsOvertime { get; init; }

        /// <summary>
        /// Gets whether the save was recorded in shootout
        /// </summary>
        public bool IsShootout { get; init; }
    }
}


