using System;
using System.Threading.Tasks;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;

namespace Application.Features.Common.MatchTimer.Services
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
