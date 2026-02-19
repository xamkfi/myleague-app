using System;
using System.Threading.Tasks;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;

namespace Application.Features.Common.MatchTimer.Services
{
    /// <summary>
    /// Persistent implementation of the match timer service with real-time updates
    /// </summary>
    public class PersistentMatchTimerService : IMatchTimerService
    {
        private readonly ITimerRepository _timerRepository;
        private readonly ITimerNotificationService _notificationService;
        private readonly ILogger<PersistentMatchTimerService> _logger;
        private readonly ITimerStore _timerStore;

        /// <summary>
        /// Initializes a new instance of the PersistentMatchTimerService class
        /// </summary>
        /// <param name="timerRepository">The timer repository</param>
        /// <param name="notificationService">The timer notification service</param>
        /// <param name="logger">The logger</param>
        public PersistentMatchTimerService(
            ITimerRepository timerRepository,
            ITimerNotificationService notificationService,
            ITimerStore timerStore,
            ILogger<PersistentMatchTimerService> logger)
        {
            _timerRepository = timerRepository;
            _notificationService = notificationService;
            _timerStore = timerStore;
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
                _logger.LogInformation("Starting timer for match {MatchId} with period {PeriodNumber}", matchId, periodNumber);
                
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

                // If period changed, reset timer to 0:00 for new period
                if (periodNumber.HasValue && 
                    timerState.PeriodNumber.HasValue &&
                    timerState.PeriodNumber.Value != periodNumber.Value)
                {
                    _logger.LogInformation(
                        "Period changed from {OldPeriod} to {NewPeriod}, resetting timer for match {MatchId}",
                        timerState.PeriodNumber, periodNumber, matchId);
                    
                    timerState.Reset();
                }

                // Set/keep initial started-at only if never started before or was reset
                if (timerState.StartedAt == null)
                {
                    timerState.StartedAt = DateTime.UtcNow;
                }

                // Update period and start via domain method (initializes runtime stopwatch and clears PausedAt)
                timerState.PeriodNumber = periodNumber;
                timerState.Start();

                _logger.LogInformation("Timer state after start - IsRunning: {IsRunning}, StartedAt: {StartedAt}, PausedAt: {PausedAt}, TotalPausedDuration: {TotalPausedDuration}", 
                    timerState.IsRunning, timerState.StartedAt, timerState.PausedAt, timerState.TotalPausedDuration);

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);
                _timerStore.Add(timerState!);
                _logger.LogInformation("Saved timer state for match {MatchId}", matchId);

                // Add a small delay to ensure the database transaction is fully committed
                // before the TimerBackgroundService reads it again (race condition fix)
                await Task.Delay(100); // 100ms delay

                TimeSpan elapsedTime = timerState.ElapsedTime;
                _logger.LogInformation("Calculated elapsed time {ElapsedTime} for match {MatchId}", elapsedTime, matchId);
                
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
                _logger.LogInformation("Stopping timer for match {MatchId}", matchId);
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                
                if (timerState == null)
                {
                    _logger.LogWarning("Timer state not found for match {MatchId}", matchId);
                    return;
                }

                _logger.LogInformation("Timer state before stop - IsRunning: {IsRunning}, StartedAt: {StartedAt}, PausedAt: {PausedAt}, TotalPausedDuration: {TotalPausedDuration}", 
                    timerState.IsRunning, timerState.StartedAt, timerState.PausedAt, timerState.TotalPausedDuration);

                // Use domain pause to fold the last running segment and stop the runtime stopwatch
                timerState.Pause();

                _logger.LogInformation("Timer state after stop - IsRunning: {IsRunning}, StartedAt: {StartedAt}, PausedAt: {PausedAt}, TotalPausedDuration: {TotalPausedDuration}", 
                    timerState.IsRunning, timerState.StartedAt, timerState.PausedAt, timerState.TotalPausedDuration);

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);
                _timerStore.TryRemove(matchId, out _);
                _logger.LogInformation("Saved stopped timer state for match {MatchId}", matchId);

                // Add a small delay to ensure the database transaction is fully committed
                // before the TimerBackgroundService reads it again (race condition fix)
                await Task.Delay(100); // 100ms delay

                // Verify the save worked by reading it back
                TimerState? savedState = await _timerRepository.GetTimerStateAsync(matchId);
                _logger.LogInformation("Verified saved state - IsRunning: {IsRunning}, StartedAt: {StartedAt}, PausedAt: {PausedAt}, TotalPausedDuration: {TotalPausedDuration}", 
                    savedState?.IsRunning, savedState?.StartedAt, savedState?.PausedAt, savedState?.TotalPausedDuration);

                TimeSpan elapsedTime = timerState.ElapsedTime;
                _logger.LogInformation("Calculated elapsed time {ElapsedTime} for stopped timer match {MatchId}", elapsedTime, matchId);
                
                // Notify clients of the timer update
                TimerUpdate update = TimerUpdate.CreateStopped(matchId, timerState.PeriodNumber, elapsedTime);
                await NotifyTimerUpdateAsync(matchId, update);
                _timerStore.TryRemove(matchId, out _);

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

                timerState.Reset();

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);
                _timerStore.TryRemove(matchId, out TimerState? removedState);

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
        /// Sets the timer to a specific elapsed time for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="elapsedTime">The elapsed time to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SetTimerAsync(Guid matchId, TimeSpan elapsedTime)
        {
            try
            {
                _logger.LogInformation("Setting timer for match {MatchId} to {ElapsedTime}", matchId, elapsedTime);
                
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                if (timerState == null)
                {
                    _logger.LogWarning("Timer does not exist for match {MatchId}, creating new timer", matchId);
                    await CreateTimerAsync(matchId);
                    timerState = await _timerRepository.GetTimerStateAsync(matchId);
                }

                DateTime now = DateTime.UtcNow;

                // Calculate the StartedAt time that would result in the desired elapsed time
                // Formula: elapsedTime = (now - StartedAt) - TotalPausedDuration
                // Rearranged: StartedAt = now - elapsedTime - TotalPausedDuration
                DateTime newStartedAt = now - elapsedTime;

                timerState!.StartedAt = newStartedAt;
                timerState.TotalPausedDuration = TimeSpan.Zero; // Reset paused duration for clean state
                timerState.LastUpdated = now;

                if (timerState!.IsRunning)
                {
                    // If timer is (or should be) running, initialize runtime stopwatch via Start()
                    timerState.PausedAt = null;
                    timerState.Start();
                }
                else
                {
                    // If paused/stopped, fix the persisted timestamps to reflect set value
                    timerState.PausedAt = now;
                    timerState.LastResumedAt = null;
                    timerState.IsRunning = false;
                }

                _logger.LogInformation("Timer state after set - IsRunning: {IsRunning}, StartedAt: {StartedAt}, PausedAt: {PausedAt}, ElapsedTime: {ElapsedTime}", 
                    timerState.IsRunning, timerState.StartedAt, timerState.PausedAt, timerState.ElapsedTime);

                await _timerRepository.SaveTimerStateAsync(matchId, timerState);
                
                // Update the timer store
                if (timerState.IsRunning)
                {
                    _timerStore.Add(timerState);
                }
                else
                {
                    _timerStore.TryRemove(matchId, out _);
                }

                // Add a small delay to ensure the database transaction is fully committed
                await Task.Delay(100);

                // Create appropriate timer update based on running state
                TimerUpdate update = timerState!.IsRunning
                    ? TimerUpdate.CreateStarted(matchId, timerState.PeriodNumber, elapsedTime)
                    : TimerUpdate.CreateStopped(matchId, timerState.PeriodNumber, elapsedTime);
                
                await NotifyTimerUpdateAsync(matchId, update);

                _logger.LogInformation("Set timer for match {MatchId} to {ElapsedTime}", matchId, elapsedTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting timer for match {MatchId} to {ElapsedTime}", matchId, elapsedTime);
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
        /// Gets the current period number for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The current period number, or null if not set</returns>
        public async Task<int?> GetCurrentPeriodTime(Guid matchId)
        {
            try
            {
                TimerState? timerState = await _timerRepository.GetTimerStateAsync(matchId);
                if (timerState == null)
                {
                    _logger.LogDebug("Timer does not exist for match {MatchId}, returning null period", matchId);
                    return null;
                }

                int? periodNumber = timerState.PeriodNumber;
                _logger.LogDebug("Current period for match {MatchId}: {PeriodNumber}", matchId, periodNumber);
                return periodNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current period for match {MatchId}", matchId);
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
                _timerStore.TryRemove(matchId, out TimerState? removedState);
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
