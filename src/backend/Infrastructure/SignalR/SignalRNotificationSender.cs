using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.SignalR
{
    /// <summary>
    /// SignalR implementation of the notification sender interface
    /// </summary>
    public class SignalRNotificationSender : INotificationSender
    {
        private readonly DomainEventNotifier _notifier;
        private readonly ILogger<SignalRNotificationSender> _logger;

        /// <summary>
        /// Initializes a new instance of the SignalRNotificationSender class
        /// </summary>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public SignalRNotificationSender(
            DomainEventNotifier notifier,
            ILogger<SignalRNotificationSender> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        /// <summary>
        /// Sends a notification with the specified event name and payload
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <param name="payload">The payload to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendNotificationAsync(string eventName, object payload)
        {
            try
            {
                _logger.LogInformation("Sending notification for event {EventName}", eventName);
                await _notifier.NotifyAsync(eventName, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for event {EventName}", eventName);
                throw;
            }
        }
    }
} 