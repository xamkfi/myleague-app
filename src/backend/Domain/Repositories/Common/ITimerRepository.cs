using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Domain.Repositories.Common
{
    /// <summary>
    /// Repository interface for managing timer states
    /// </summary>
    public interface ITimerRepository
    {
        /// <summary>
        /// Gets the timer state for a specific match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The timer state, or null if not found</returns>
        Task<TimerState?> GetTimerStateAsync(Guid matchId);

        /// <summary>
        /// Saves the timer state for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="timerState">The timer state to save</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SaveTimerStateAsync(Guid matchId, TimerState timerState);

        /// <summary>
        /// Deletes the timer state for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DeleteTimerStateAsync(Guid matchId);

        /// <summary>
        /// Gets all active timer states
        /// </summary>
        /// <returns>A collection of active timer states</returns>
        Task<IEnumerable<TimerState>> GetActiveTimersAsync();

        /// <summary>
        /// Checks if a timer exists for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>True if the timer exists, false otherwise</returns>
        Task<bool> ExistsAsync(Guid matchId);
    }
} 