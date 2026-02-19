using System;
using System.Threading.Tasks;
using Application.DTOs.Common;

namespace Application.Services.Common
{
    /// <summary>
    /// Service interface for sending timer notifications
    /// </summary>
    public interface ITimerNotificationService
    {
        /// <summary>
        /// Sends a timer update notification
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="update">The timer update to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task NotifyTimerUpdateAsync(Guid matchId, TimerUpdate update);
    }
} 