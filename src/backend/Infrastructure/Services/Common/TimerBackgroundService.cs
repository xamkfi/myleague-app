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
                _logger.LogDebug("TimerBackgroundService: Starting periodic update cycle");
                
                // Get all running timers
                IEnumerable<Domain.Entities.Common.TimerState> runningTimers = await timerRepository.GetActiveTimersAsync();
                
                if (!runningTimers.Any())
                {
                    _logger.LogDebug("TimerBackgroundService: No running timers found for periodic updates");
                    return; // No running timers to update
                }

                _logger.LogInformation("TimerBackgroundService: Sending periodic updates for {Count} running timers", runningTimers.Count());

                foreach (Domain.Entities.Common.TimerState timerState in runningTimers)
                {
                    try
                    {
                        // Calculate current elapsed time
                        TimeSpan elapsedTime = timerState.ElapsedTime;
                        
                        _logger.LogInformation("TimerBackgroundService: Timer state for match {MatchId}: IsRunning={IsRunning}, ElapsedTime={ElapsedTime}", 
                            timerState.MatchId, timerState.IsRunning, elapsedTime);
                        
                        // Create periodic update
                        TimerUpdate update = TimerUpdate.CreateUpdate(
                            timerState.MatchId, 
                            timerState.PeriodNumber, 
                            elapsedTime, 
                            timerState.IsRunning);
                        
                        // Send update
                        await notificationService.NotifyTimerUpdateAsync(timerState.MatchId, update);
                        
                        _logger.LogInformation("TimerBackgroundService: Sent periodic update for match {MatchId}: {ElapsedTime}", 
                            timerState.MatchId, elapsedTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "TimerBackgroundService: Error sending periodic update for match {MatchId}", timerState.MatchId);
                    }
                }
                
                _logger.LogDebug("TimerBackgroundService: Completed periodic update cycle");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TimerBackgroundService: Error getting running timers for periodic updates");
            }
        }
    }
} 