using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.DTOs.Notifications;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;
using MyLeague.Infrastructure.SignalR.Sports.Floorball;
using System.Threading;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPenaltyAssignedEvent by notifying SignalR clients with penalty details.
    /// </summary>
    public class FloorballPenaltyAssignedEventHandler : NotificationDomainEventHandler<FloorballPenaltyAssignedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPenaltyAssignedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballPenaltyAssignedEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballPenaltyAssignedEventHandler> logger)
            : base(notificationSender, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Builds the notification payload from the domain event
        /// </summary>
        /// <param name="domainEvent">The domain event</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A tuple containing the event name and notification payload</returns>
        protected override async Task<(string EventName, object? Notification)> BuildNotificationAsync(
            FloorballPenaltyAssignedEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId, cancellationToken);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for PenaltyAssigned event.", domainEvent.MatchId);
                return (FloorballNotificationEvents.PenaltyAssigned, null);
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";

            FloorballPenaltyAssignedNotification notification = new()
            {
                MatchId = domainEvent.MatchId,
                EventTime = domainEvent.OccurredOn,
                PenaltyType = domainEvent.PenaltyType.ToString(),
                TeamId = domainEvent.TeamId,
                PlayerId = domainEvent.PlayerId!.Value,
                HomeTeam = new FloorballPenaltyAssignedNotification.TeamInfo 
                { 
                    Id = match.HomeTeam?.Id, 
                    Name = homeTeamName 
                },
                AwayTeam = new FloorballPenaltyAssignedNotification.TeamInfo 
                { 
                    Id = match.AwayTeam?.Id, 
                    Name = awayTeamName 
                }
            };

            _logger.LogInformation("Penalty assigned in match between {HomeTeam} and {AwayTeam}", 
                homeTeamName, awayTeamName);

            return (FloorballNotificationEvents.PenaltyAssigned, notification);
        }
    }
} 
