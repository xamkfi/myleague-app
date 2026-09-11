using System.Collections.Concurrent;
using System.Text.Json;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.MatchTimer.Services;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.Services.Common
{
    public class TimerNotificationService : ITimerNotificationService
    {
        private readonly DomainEventNotifier _notifier;
        private readonly ILogger<TimerNotificationService> _logger;
        private readonly ConcurrentDictionary<Guid, long> _sequences = new();

        public TimerNotificationService(
            DomainEventNotifier notifier,
            ILogger<TimerNotificationService> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public async Task NotifyTimerUpdateAsync(Guid matchId, TimerUpdate update)
        {
            try
            {
                long seq = _sequences.AddOrUpdate(matchId, 1, (_, prev) => prev + 1);
                update.Sequence = seq;
                string payloadJson = JsonSerializer.Serialize(update);
                await _notifier.NotifyMatchGroupAsync(matchId, "TimerUpdateEvent", payloadJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send timer update for match {MatchId}", matchId);
                throw;
            }
        }
    }
} 