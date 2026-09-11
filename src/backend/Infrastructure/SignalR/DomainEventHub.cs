using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Application.Features.Common.MatchTimer.Services;

namespace MyLeague.Infrastructure.SignalR
{
    public class DomainEventHub : Hub
    {
        private readonly DomainEventNotifier _notifier;
        private readonly ILogger<DomainEventHub> _logger;
        private readonly IMatchTimerService _timerService;

        public DomainEventHub(DomainEventNotifier notifier, ILogger<DomainEventHub> logger, IMatchTimerService timerService)
        {
            _notifier = notifier;
            _logger = logger;
            _timerService = timerService;
        }

        public string GetConnectionId() => Context.ConnectionId;

        public async Task SubscribeToEventTypeAsync(string eventType)
        {
            await _notifier.SubscribeToEventTypeAsync(Context.ConnectionId, eventType);
        }

        public async Task UnsubscribeFromEventTypeAsync(string eventType)
        {
            await _notifier.UnsubscribeFromEventTypeAsync(Context.ConnectionId, eventType);
        }

        public async Task SubscribeToMatchAsync(Guid matchId)
        {
            await _notifier.SubscribeToMatchAsync(Context.ConnectionId, matchId);
        }

        public async Task UnsubscribeFromMatchAsync(Guid matchId)
        {
            await _notifier.UnsubscribeFromMatchAsync(Context.ConnectionId, matchId);
        }

        [Authorize]
        public async Task StartTimerAsync(Guid matchId, int? periodNumber = null)
        {
            _logger.LogInformation("Timer start requested for match {MatchId} by {User}", matchId, Context.UserIdentifier);
            await _timerService.StartTimerAsync(matchId, periodNumber);
        }

        [Authorize]
        public async Task StopTimerAsync(Guid matchId)
        {
            _logger.LogInformation("Timer stop requested for match {MatchId} by {User}", matchId, Context.UserIdentifier);
            await _timerService.StopTimerAsync(matchId);
        }

        [Authorize]
        public async Task ResetTimerAsync(Guid matchId)
        {
            _logger.LogInformation("Timer reset requested for match {MatchId} by {User}", matchId, Context.UserIdentifier);
            await _timerService.ResetTimerAsync(matchId);
        }
    }
}
