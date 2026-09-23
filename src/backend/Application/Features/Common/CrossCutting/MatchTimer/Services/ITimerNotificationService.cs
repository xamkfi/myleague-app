using System;
using System.Threading.Tasks;
using Application.Features.Common.Organization.Users.DTOs;
using Application.Features.Common.Organization.Persons.DTOs;
using Application.Features.Common.Organization.Clubs.DTOs;
using Application.Features.Common.Organization.Divisions.DTOs;
using Application.Features.Common.Content.News.DTOs;
using Application.Features.Common.CrossCutting.Search.DTOs;
using Application.Features.Common.CrossCutting.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;

namespace Application.Features.Common.CrossCutting.MatchTimer.Services
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
