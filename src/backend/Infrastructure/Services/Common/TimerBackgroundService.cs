using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.Services.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyLeague.Infrastructure.Services.Common
{
    /// <summary>
    /// Background service that sends periodic timer updates for running timers
    /// </summary>
    public class TimerBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TimerBackgroundService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _orphanedCleanupInterval = TimeSpan.FromMinutes(5); // Clean up orphaned timers every 5 minutes
        private DateTime _lastOrphanedCleanup = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the TimerBackgroundService class
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="logger">The logger</param>
        public TimerBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TimerBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Executes the background service
        /// </summary>
        /// <param name="stoppingToken">The cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timer background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendPeriodicTimerUpdatesAsync();
                    
                    // Check if it's time to clean up orphaned timers
                    bool shouldCleanupOrphaned = DateTime.UtcNow - _lastOrphanedCleanup >= _orphanedCleanupInterval;
                    if (shouldCleanupOrphaned)
                    {
                        await CleanupOrphanedTimersAsync();
                        _lastOrphanedCleanup = DateTime.UtcNow;
                    }
                    
                    await Task.Delay(_updateInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service is being stopped
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in timer background service");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Timer background service stopped");
        }

        /// <summary>
        /// Sends periodic updates for all running timers
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task SendPeriodicTimerUpdatesAsync()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            ITimerRepository timerRepository = scope.ServiceProvider.GetRequiredService<ITimerRepository>();
            ITimerNotificationService notificationService = scope.ServiceProvider.GetRequiredService<ITimerNotificationService>();

            try
            {
                // Get all running timers
                IEnumerable<Domain.Entities.Common.TimerState> runningTimers = await timerRepository.GetActiveTimersAsync();
                
                if (!runningTimers.Any())
                {
                    return; // No running timers to update
                }

                foreach (Domain.Entities.Common.TimerState timerState in runningTimers)
                {
                    try
                    {
                        // Calculate current elapsed time
                        TimeSpan elapsedTime = timerState.ElapsedTime;
                        
                        // Create periodic update
                        TimerUpdate update = TimerUpdate.CreateUpdate(
                            timerState.MatchId, 
                            timerState.PeriodNumber, 
                            elapsedTime, 
                            timerState.IsRunning);
                        
                        // Send update
                        await notificationService.NotifyTimerUpdateAsync(timerState.MatchId, update);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "TimerBackgroundService: Error sending periodic update for match {MatchId}", timerState.MatchId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TimerBackgroundService: Error getting running timers for periodic updates");
            }
        }

        /// <summary>
        /// Cleans up orphaned timers that are no longer associated with active matches
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task CleanupOrphanedTimersAsync()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            ITimerRepository timerRepository = scope.ServiceProvider.GetRequiredService<ITimerRepository>();

            try
            {
                _logger.LogInformation("TimerBackgroundService: Starting orphaned timer cleanup");

                // Get all timers (not just active ones)
                IEnumerable<Domain.Entities.Common.TimerState> allTimers = await timerRepository.GetAllTimersAsync();
                int orphanedCount = 0;

                foreach (Domain.Entities.Common.TimerState timerState in allTimers)
                {
                    try
                    {
                        // Check if the associated match exists and is still in progress
                        bool isOrphaned = await IsTimerOrphanedAsync(timerState.MatchId);
                        
                        if (isOrphaned)
                        {
                            _logger.LogInformation("TimerBackgroundService: Found orphaned timer for match {MatchId}, destroying it", timerState.MatchId);
                            await timerRepository.DeleteTimerStateAsync(timerState.MatchId);
                            orphanedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "TimerBackgroundService: Error processing timer for match {MatchId} during orphaned cleanup", timerState.MatchId);
                    }
                }

                if (orphanedCount > 0)
                {
                    _logger.LogInformation("TimerBackgroundService: Cleaned up {Count} orphaned timers", orphanedCount);
                }
                else
                {
                    _logger.LogDebug("TimerBackgroundService: No orphaned timers found during cleanup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TimerBackgroundService: Error during orphaned timer cleanup");
            }
        }

        /// <summary>
        /// Checks if a timer is orphaned (no longer associated with an active match)
        /// </summary>
        /// <param name="matchId">The match ID to check</param>
        /// <returns>True if the timer is orphaned, false otherwise</returns>
        private async Task<bool> IsTimerOrphanedAsync(Guid matchId)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            
            try
            {
                // Try to get the match from the floorball repository
                var floorballMatchRepository = scope.ServiceProvider.GetService<Domain.Repositories.Floorball.IFloorballMatchRepository>();
                if (floorballMatchRepository != null)
                {
                    var match = await floorballMatchRepository.GetByIdAsync(matchId);
                    if (match != null)
                    {
                        // Timer is orphaned if match is completed
                        return match.Status == Domain.Enums.Floorball.FloorballMatchStatus.Completed;
                    }
                }

                // Try to get the match from the event-sourced repository
                var eventSourcedMatchRepository = scope.ServiceProvider.GetService<Domain.Repositories.Floorball.IEventSourcedFloorballMatchRepository>();
                if (eventSourcedMatchRepository != null)
                {
                    var eventSourcedMatch = await eventSourcedMatchRepository.GetByIdAsync(matchId, CancellationToken.None);
                    if (eventSourcedMatch != null)
                    {
                        // Timer is orphaned if match is completed
                        return eventSourcedMatch.Status == Domain.Enums.Floorball.FloorballMatchStatus.Completed;
                    }
                }

                // If we can't find the match at all, consider the timer orphaned
                _logger.LogWarning("TimerBackgroundService: Could not find match {MatchId} in any repository, considering timer orphaned", matchId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TimerBackgroundService: Error checking if timer is orphaned for match {MatchId}", matchId);
                // In case of error, don't delete the timer - err on the side of caution
                return false;
            }
        }
    }
} 