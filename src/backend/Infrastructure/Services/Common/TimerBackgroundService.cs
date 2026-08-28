using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Configuration;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.MatchTimer.Services;
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
        /// Gets the duration limit in seconds for a given period.
        /// Currently uses the global PeriodDurationConfiguration as a fallback.
        /// TODO: When re-enabled, look up per-match FloorballMatchRules from the match entity
        /// to support dynamic period durations configured per season.
        /// </summary>
        /// <param name="periodNumber">The period number</param>
        /// <returns>Duration limit in seconds, or 0 if no limit</returns>
        private int GetPeriodDurationLimit(int? periodNumber)
        {
            // Fallback to global configuration.
            // Per-match rules (FloorballMatch.MatchRules) are the authoritative source
            // and are used by the frontend and StartPeriodHandler.
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
