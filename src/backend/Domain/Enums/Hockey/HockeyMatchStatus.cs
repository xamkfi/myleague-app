
namespace Domain.Enums.Hockey
{
    /// <summary>
    /// Represents the status of a hockey match
    /// </summary>
    public enum HockeyMatchStatus
    {
        /// <summary>
        /// No status assigned
        /// </summary>
        None = 0,

        /// <summary>
        /// Match is scheduled but has not started
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Match is postponed to a later date
        /// </summary>
        Postponed = 2,

        /// <summary>
        /// Match is currently in progress
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Match has been completed
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Match has been cancelled
        /// </summary>
        Cancelled = 5

    }
}
