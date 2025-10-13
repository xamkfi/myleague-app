using System;
using System.Threading.Tasks;

namespace Application.Services.Common
{
    /// <summary>
    /// Service interface for managing persistent match timers with real-time updates
    /// </summary>
    public interface IMatchTimerService
    {
        /// <summary>
        /// Creates a new timer instance for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task CreateTimerAsync(Guid matchId);

        /// <summary>
        /// Starts the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="periodNumber">Optional period number</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task StartTimerAsync(Guid matchId, int? periodNumber = null);

        /// <summary>
        /// Stops the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task StopTimerAsync(Guid matchId);

        /// <summary>
        /// Resets the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task ResetTimerAsync(Guid matchId);

        /// <summary>
        /// Sets the timer to a specific elapsed time for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="elapsedTime">The elapsed time to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetTimerAsync(Guid matchId, TimeSpan elapsedTime);

        /// <summary>
        /// Gets the elapsed time for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The elapsed time</returns>
        Task<TimeSpan> GetElapsedTimeAsync(Guid matchId);

        /// <summary>
        /// Gets the current period number for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The current period number, or null if not set</returns>
        Task<int?> GetCurrentPeriodTime(Guid matchId);

        /// <summary>
        /// Checks if the timer is running for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>True if the timer is running, false otherwise</returns>
        Task<bool> IsRunningAsync(Guid matchId);

        /// <summary>
        /// Checks if a timer exists for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>True if the timer exists, false otherwise</returns>
        Task<bool> ExistsAsync(Guid matchId);

        /// <summary>
        /// Destroys the timer instance for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DestroyTimerAsync(Guid matchId);
    }
} 
