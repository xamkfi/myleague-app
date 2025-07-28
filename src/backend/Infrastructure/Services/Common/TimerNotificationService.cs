using System;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.Services.Common;
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
                _logger.LogDebug("Sending timer update for match {MatchId}: {EventType}", matchId, update.EventType);

                // Send to match-specific group
                await _notifier.NotifyMatchAsync(matchId, "TimerUpdateEvent", update);

                // Send to general timer event subscribers
                await _notifier.NotifyAsync("TimerUpdateEvent", update);

                _logger.LogDebug("Successfully sent timer update for match {MatchId}", matchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending timer update for match {MatchId}", matchId);
                throw;
            }
        }
    }
} 