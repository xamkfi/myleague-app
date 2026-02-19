using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.MatchTimer.Services;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.Services.Common
{
    /// <summary>
    /// Implementation of the timer notification service using SignalR
    /// </summary>
    public class TimerNotificationService : ITimerNotificationService
    {
        private readonly DomainEventNotifier _notifier;
        private readonly ILogger<TimerNotificationService> _logger;
        private readonly ConcurrentDictionary<Guid, long> _sequences = new();

        /// <summary>
        /// Initializes a new instance of the TimerNotificationService class
        /// </summary>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public TimerNotificationService(
            DomainEventNotifier notifier,
            ILogger<TimerNotificationService> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        /// <summary>
        /// Sends a timer update notification via SignalR
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="update">The timer update to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task NotifyTimerUpdateAsync(Guid matchId, TimerUpdate update)
        {
            try
            {
                // Assign a monotonically increasing sequence for every match across ALL notifications.
                // This guarantees consistent ordering regardless of the event source.
                long seq = _sequences.AddOrUpdate(matchId, 1, (_, prev) => prev + 1);
                update.Sequence = seq;
                // Send to match-specific group
                await _notifier.NotifyMatchAsync(matchId, "TimerUpdateEvent", update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TimerNotificationService: Failed to send timer update for match {MatchId}", matchId);
                throw;
            }
        }
    }
} 