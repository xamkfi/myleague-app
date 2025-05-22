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
    /// Handles FloorballGoalScoredEvent by notifying SignalR clients with goal details.
    /// </summary>
    public class FloorballGoalScoredEventHandler : NotificationDomainEventHandler<FloorballGoalScoredEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballGoalScoredEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballGoalScoredEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballGoalScoredEventHandler> logger)
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
            FloorballGoalScoredEvent domainEvent, 
            CancellationToken cancellationToken = default)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId, cancellationToken);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for GoalScored event.", domainEvent.MatchId);
                return (FloorballNotificationEvents.GoalScored, null);
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";
            string scoringTeamName = domainEvent.TeamId == match.HomeTeam?.Id ? homeTeamName : awayTeamName;

            // Create a FloorballGoalScoredNotification object initializing all properties at once
            FloorballGoalScoredNotification notification = new()
            {
                MatchId = domainEvent.MatchId,
                TeamId = domainEvent.TeamId,
                PlayerId = domainEvent.PlayerId!.Value,
                PeriodNumber = domainEvent.PeriodNumber,
                EventTime = domainEvent.OccurredOn,
                HomeTeam = new TeamInfo 
                { 
                    Id = match.HomeTeamId, 
                    Name = homeTeamName 
                },
                AwayTeam = new TeamInfo 
                { 
                    Id = match.AwayTeamId, 
                    Name = awayTeamName 
                }
            };

            _logger.LogInformation("Goal scored by {TeamName} in period {PeriodNumber}. Match: {HomeTeam} vs {AwayTeam}", 
                scoringTeamName, domainEvent.PeriodNumber, homeTeamName, awayTeamName);

            return (FloorballNotificationEvents.GoalScored, notification);
        }
    }
} 

