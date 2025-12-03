using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Common
{
    /// <summary>
    /// Service for sending notifications
    /// </summary>
    public interface INotificationSenderService
    {
        /// <summary>
        /// Sends a notification with the specified event name and payload
        /// </summary>
        Task SendNotificationAsync(string eventName, object payload);
    }
}
