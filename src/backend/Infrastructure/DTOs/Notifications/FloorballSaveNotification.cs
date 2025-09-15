namespace MyLeague.Infrastructure.DTOs.Notifications
{
    /// <summary>
    /// DTO for the FloorballSave notification
    /// </summary>
    public class FloorballSaveNotification
    {
        /// <summary>
        /// Gets the ID of the match
        /// </summary>
        public Guid MatchId { get; set; }

        /// <summary>
        /// Gets the ID of the team that made the save
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// Gets the ID of the goalie who made the save
        /// </summary>
        public Guid GoalieId { get; set; }

        /// <summary>
        /// Gets the name of the goalie who made the save
        /// </summary>
        public string GoalieName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the period number when the save was made
        /// </summary>
        public int PeriodNumber { get; set; }

        /// <summary>
        /// Gets the time in seconds within the period when the save was made
        /// </summary>
        public int TimeInSeconds { get; set; }

        /// <summary>
        /// Gets a value indicating whether the save occurred during overtime
        /// </summary>
        public bool IsOvertime { get; set; }

        /// <summary>
        /// Gets a value indicating whether the save occurred during a shootout
        /// </summary>
        public bool IsShootout { get; set; }
    }
}
