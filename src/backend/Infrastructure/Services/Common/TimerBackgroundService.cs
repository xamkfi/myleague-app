using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Configuration;
using Application.DTOs.Common;
using Application.Services.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MyLeague.Infrastructure.Services.Common
{
    /// <summary>
    /// Background service that sends periodic timer updates for running timers
    /// </summary>
    public class TimerBackgroundService : BackgroundService
    {
        private readonly ITimerStore _timerStore;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TimerBackgroundService> _logger;
        private readonly PeriodDurationConfiguration _periodConfig;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Initializes a new instance of the TimerBackgroundService class
        /// </summary>
        /// <param name="timerStore">The timer store</param>
        /// <param name="scopeFactory">The service scope factory</param>
        /// <param name="periodConfig">The period duration configuration</param>
        /// <param name="logger">The logger</param>
        public TimerBackgroundService(
            ITimerStore timerStore,
            IServiceScopeFactory scopeFactory,
            IOptions<PeriodDurationConfiguration> periodConfig,
            ILogger<TimerBackgroundService> logger)
        {
            _timerStore = timerStore;
            _scopeFactory = scopeFactory;
            _periodConfig = periodConfig.Value;
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

            // Use a stopwatch-anchored schedule to avoid drift from Task.Delay processing time
            System.Diagnostics.Stopwatch scheduler = System.Diagnostics.Stopwatch.StartNew();
            long tick = 0; // number of full intervals elapsed

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendPeriodicTimerUpdatesAsync();

                    tick++;
                    // Compute the exact next due time based on the anchor
                    TimeSpan target = TimeSpan.FromTicks(_updateInterval.Ticks * tick);
                    TimeSpan delay = target - scheduler.Elapsed;
                    if (delay < TimeSpan.Zero)
                    {
                        // We are behind schedule; catch up without additional delay
                        delay = TimeSpan.Zero;
                    }
                    await Task.Delay(delay, stoppingToken);
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
            // TODO: Re-enable periodic timer updates when needed
            return;
            
            try
            {
                _logger.LogDebug("TimerBackgroundService: Starting periodic update cycle");
                // Get snapshot of active timers to avoid collection modification issues during iteration
                IEnumerable<Domain.Entities.Common.TimerState> runningTimers = _timerStore.GetActive().ToList();
                
                if (!runningTimers.Any())
                {
                    return; // No running timers to update
                }
                _logger.LogInformation("TimerBackgroundService: Sending periodic updates for {Count} running timers", runningTimers.Count());

                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    ITimerNotificationService notificationService = scope.ServiceProvider.GetRequiredService<ITimerNotificationService>();
                    IMatchTimerService timerService = scope.ServiceProvider.GetRequiredService<IMatchTimerService>();

                    foreach (Domain.Entities.Common.TimerState timerState in runningTimers)
                    {
                        try
                        {
                            timerState.Tick();
                            TimeSpan elapsedTime = timerState.ElapsedTime;
                            
                            _logger.LogInformation("TimerBackgroundService: Timer state for match {MatchId}: IsRunning={IsRunning}, ElapsedTime={ElapsedTime}, Period={Period}",
                                timerState.MatchId, timerState.IsRunning, elapsedTime, timerState.PeriodNumber);
                            
                            // Check if period duration limit reached
                            int durationLimit = GetPeriodDurationLimit(timerState.PeriodNumber);
                            
                            if (durationLimit > 0 && elapsedTime.TotalSeconds >= durationLimit)
                            {
                                _logger.LogInformation(
                                    "Auto-stopping timer for match {MatchId} period {Period} at limit {Limit}s (elapsed: {Elapsed}s)",
                                    timerState.MatchId, timerState.PeriodNumber, durationLimit, elapsedTime.TotalSeconds);
                                
                                await timerService.StopTimerAsync(timerState.MatchId);
                                continue; // Skip sending update - StopTimerAsync will send stopped event
                            }
                            
                            TimerUpdate update = TimerUpdate.CreateUpdate(
                                timerState.MatchId,
                                timerState.PeriodNumber,
                                elapsedTime,
                                timerState.IsRunning);
                            await notificationService.NotifyTimerUpdateAsync(timerState.MatchId, update);
                            
                            _logger.LogInformation("TimerBackgroundService: Sent periodic update for match {MatchId}: {ElapsedTime}",
                                timerState.MatchId, elapsedTime);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "TimerBackgroundService: Error sending periodic update for match {MatchId}", timerState.MatchId);
                        }
                    }
                }
                _logger.LogDebug("TimerBackgroundService: Completed periodic update cycle");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TimerBackgroundService: Error getting running timers for periodic updates");
            }
        }

        /// <summary>
        /// Gets the duration limit in seconds for a given period
        /// </summary>
        /// <param name="periodNumber">The period number</param>
        /// <returns>Duration limit in seconds, or 0 if no limit</returns>
        private int GetPeriodDurationLimit(int? periodNumber)
        {
            return periodNumber switch
            {
                1 or 2 => _periodConfig.RegularPeriodSeconds,
                3 => _periodConfig.OvertimePeriodSeconds,
                4 => _periodConfig.ShootoutPeriodSeconds,
                _ => 0 // No limit for unknown periods
            };
        }
    }
} 