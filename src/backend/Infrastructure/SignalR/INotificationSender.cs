using System.Threading.Tasks;

namespace MyLeague.Infrastructure.SignalR
{
    /// <summary>
    /// Interface for sending notifications to clients
    /// </summary>
    public interface INotificationSender
    {
        /// <summary>
        /// Sends a notification with the specified event name and payload
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <param name="payload">The payload to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendNotificationAsync(string eventName, object payload);
    }
} 