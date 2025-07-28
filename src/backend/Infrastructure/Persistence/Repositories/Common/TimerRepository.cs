using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the timer repository using Entity Framework
    /// </summary>
    public class TimerRepository : ITimerRepository
    {
        private readonly CommonDbContext _dbContext;
        private readonly ILogger<TimerRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the TimerRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="logger">The logger</param>
        public TimerRepository(CommonDbContext dbContext, ILogger<TimerRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Gets the timer state for a specific match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The timer state, or null if not found</returns>
        public async Task<TimerState?> GetTimerStateAsync(Guid matchId)
        {
            try
            {
                TimerState? timerState = await _dbContext.TimerStates
                    .FirstOrDefaultAsync(t => t.MatchId == matchId);

                _logger.LogDebug("Retrieved timer state for match {MatchId}: {IsRunning}", 
                    matchId, timerState?.IsRunning ?? false);

                return timerState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timer state for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Saves the timer state for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="timerState">The timer state to save</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveTimerStateAsync(Guid matchId, TimerState timerState)
        {
            try
            {
                timerState.MatchId = matchId;
                timerState.LastUpdated = DateTime.UtcNow;

                TimerState? existingState = await _dbContext.TimerStates
                    .FirstOrDefaultAsync(t => t.MatchId == matchId);

                if (existingState == null)
                {
                    _dbContext.TimerStates.Add(timerState);
                    _logger.LogInformation("Created new timer state for match {MatchId}", matchId);
                }
                else
                {
                    _dbContext.Entry(existingState).CurrentValues.SetValues(timerState);
                    _logger.LogInformation("Updated timer state for match {MatchId}", matchId);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving timer state for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Deletes the timer state for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task DeleteTimerStateAsync(Guid matchId)
        {
            try
            {
                TimerState? timerState = await _dbContext.TimerStates
                    .FirstOrDefaultAsync(t => t.MatchId == matchId);

                if (timerState != null)
                {
                    _dbContext.TimerStates.Remove(timerState);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Deleted timer state for match {MatchId}", matchId);
                }
                else
                {
                    _logger.LogWarning("Timer state not found for match {MatchId} during deletion", matchId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timer state for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Gets all active timer states
        /// </summary>
        /// <returns>A collection of active timer states</returns>
        public async Task<IEnumerable<TimerState>> GetActiveTimersAsync()
        {
            try
            {
                List<TimerState> activeTimers = await _dbContext.TimerStates
                    .Where(t => t.IsRunning)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} active timers", activeTimers.Count);
                return activeTimers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active timers");
                throw;
            }
        }

        /// <summary>
        /// Checks if a timer exists for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>True if the timer exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid matchId)
        {
            try
            {
                bool exists = await _dbContext.TimerStates
                    .AnyAsync(t => t.MatchId == matchId);

                _logger.LogDebug("Timer exists for match {MatchId}: {Exists}", matchId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if timer exists for match {MatchId}", matchId);
                throw;
            }
        }
    }
} 