using System;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;

namespace Application.Services.Common
{
    /// <summary>
    /// Persistent implementation of the match timer service with real-time updates
    /// </summary>
    public class PersistentMatchTimerService : IMatchTimerService
    {
        private readonly ITimerRepository _timerRepository;
        private readonly ITimerNotificationService _notificationService;
        private readonly ILogger<PersistentMatchTimerService> _logger;

        /// <summary>
        /// Initializes a new instance of the PersistentMatchTimerService class
        /// </summary>
        /// <param name="timerRepository">The timer repository</param>
        /// <param name="notificationService">The timer notification service</param>
        /// <param name="logger">The logger</param>
        public PersistentMatchTimerService(
            ITimerRepository timerRepository,
            ITimerNotificationService notificationService,
            ILogger<PersistentMatchTimerService> logger)
        {
            _timerRepository = timerRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new timer instance for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task CreateTimerAsync(Guid matchId)
        {
            try
            {
                bool exists = await _timerRepository.ExistsAsync(matchId);
                if (exists)
                {
                    _logger.LogWarning("Timer already exists for match {MatchId}", matchId);
                    return;
                }

                TimerState timerState = new TimerState
                {
                    MatchId = matchId,
                    PeriodNumber = null,
                    StartedAt = null,
                    PausedAt = null,
                    TotalPausedDuration = TimeSpan.Zero,
                    IsRunning = false,
                    LastUpdated = DateTime.UtcNow
                };

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);
                _logger.LogInformation("Created timer for match {MatchId}", matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timer for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Starts the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="periodNumber">Optional period number</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task StartTimerAsync(Guid matchId, int? periodNumber = null)
        {
            try
            {
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                if (timerState == null)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}, creating new timer", matchId);
                    await CreateTimerAsync(matchId);
                    timerState = await _timerRepository.GetTimerStateAsync(matchId);
                }

                if (timerState!.IsRunning)
                {
                    _logger.LogWarning("Timer is already running for match {MatchId}", matchId);
                    return;
                }

                DateTime now = DateTime.UtcNow;
                timerState.IsRunning = true;
                
                // Only set StartedAt if this is the first start (StartedAt is null)
                // This preserves the original start time for pause/resume cycles
                if (timerState.StartedAt == null)
                {
                    timerState.StartedAt = now;
                }
                
                timerState.PeriodNumber = periodNumber;
                timerState.LastUpdated = now;

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);

                TimeSpan elapsedTime = timerState.ElapsedTime;
                TimerUpdate update = TimerUpdate.CreateStarted(matchId, periodNumber, elapsedTime);
                await NotifyTimerUpdateAsync(matchId, update);

                _logger.LogInformation("Started timer for match {MatchId} with period {PeriodNumber}", matchId, periodNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting timer for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Stops the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task StopTimerAsync(Guid matchId)
        {
            try
            {
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                if (timerState == null)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}", matchId);
                    return;
                }

                if (!timerState.IsRunning)
                {
                    _logger.LogWarning("Timer is not running for match {MatchId}", matchId);
                    return;
                }

                DateTime now = DateTime.UtcNow;
                timerState.IsRunning = false;
                timerState.PausedAt = now;
                timerState.TotalPausedDuration += now - (timerState.StartedAt ?? now);
                timerState.LastUpdated = now;

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);

                TimeSpan elapsedTime = timerState.ElapsedTime;
                TimerUpdate update = TimerUpdate.CreateStopped(matchId, timerState.PeriodNumber, elapsedTime);
                await NotifyTimerUpdateAsync(matchId, update);

                _logger.LogInformation("Stopped timer for match {MatchId}", matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping timer for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Resets the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ResetTimerAsync(Guid matchId)
        {
            try
            {
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                if (timerState == null)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}", matchId);
                    return;
                }

                timerState.StartedAt = null;
                timerState.PausedAt = null;
                timerState.TotalPausedDuration = TimeSpan.Zero;
                timerState.IsRunning = false;
                timerState.LastUpdated = DateTime.UtcNow;

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);

                TimerUpdate update = TimerUpdate.CreateReset(matchId, timerState.PeriodNumber);
                await NotifyTimerUpdateAsync(matchId, update);

                _logger.LogInformation("Reset timer for match {MatchId}", matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting timer for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Gets the elapsed time for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The elapsed time</returns>
        public async Task<TimeSpan> GetElapsedTimeAsync(Guid matchId)
        {
            try
            {
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                if (timerState == null)
                {
                    _logger.LogDebug("Timer does not exist for match {MatchId}, returning zero elapsed time", matchId);
                    return TimeSpan.Zero;
                }

                TimeSpan elapsedTime = timerState.ElapsedTime;
                _logger.LogDebug("Elapsed time for match {MatchId}: {ElapsedTime}", matchId, elapsedTime);
                return elapsedTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting elapsed time for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Checks if the timer is running for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>True if the timer is running, false otherwise</returns>
        public async Task<bool> IsRunningAsync(Guid matchId)
        {
            try
            {
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                bool isRunning = timerState?.IsRunning ?? false;
                _logger.LogDebug("Timer running status for match {MatchId}: {IsRunning}", matchId, isRunning);
                return isRunning;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if timer is running for match {MatchId}", matchId);
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
                bool exists = await _timerRepository.ExistsAsync(matchId);
                _logger.LogDebug("Timer exists for match {MatchId}: {Exists}", matchId, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if timer exists for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Destroys the timer instance for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task DestroyTimerAsync(Guid matchId)
        {
            try
            {
                bool exists = await _timerRepository.ExistsAsync(matchId);
                if (!exists)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId} during destruction", matchId);
                    return;
                }

                await _timerRepository.DeleteTimerStateAsync(matchId);
                _logger.LogInformation("Destroyed timer for match {MatchId}", matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error destroying timer for match {MatchId}", matchId);
                throw;
            }
        }

        /// <summary>
        /// Notifies clients of timer updates via SignalR
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="update">The timer update to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task NotifyTimerUpdateAsync(Guid matchId, TimerUpdate update)
        {
            try
            {
                await _notificationService.NotifyTimerUpdateAsync(matchId, update);
                _logger.LogDebug("Sent timer update for match {MatchId}: {EventType}", matchId, update.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending timer update for match {MatchId}", matchId);
                // Don't throw - SignalR failures shouldn't break timer operations
            }
        }
    }
} 