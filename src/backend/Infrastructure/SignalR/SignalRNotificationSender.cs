using Microsoft.Extensions.Logging;
using System.Text.Json;
using Application.Interfaces.Common;

namespace MyLeague.Infrastructure.SignalR
{
    public class SignalRNotificationSender : INotificationSenderService
    {
        private readonly DomainEventNotifier _notifier;
        private readonly ILogger<SignalRNotificationSender> _logger;

        public SignalRNotificationSender(
            DomainEventNotifier notifier,
            ILogger<SignalRNotificationSender> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string eventName, object payload)
        {
            try
            {
                _logger.LogInformation("Sending notification for event {EventName}", eventName);
                string payloadJson = JsonSerializer.Serialize(payload);

                await _notifier.NotifyEventGroupAsync(eventName, payloadJson);

                if (payload is IMatchNotification matchNotification)
                {
                    await _notifier.NotifyMatchGroupAsync(matchNotification.MatchId, eventName, payloadJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for event {EventName}", eventName);
                throw;
            }
        }
    }
} 
