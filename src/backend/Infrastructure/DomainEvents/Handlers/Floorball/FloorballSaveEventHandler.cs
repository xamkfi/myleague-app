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
    /// Handles FloorballSaveEvent by notifying SignalR clients with save details.
    /// </summary>
    public class FloorballSaveEventHandler : NotificationDomainEventHandler<FloorballSaveEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSaveEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballSaveEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballSaveEventHandler> logger)
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
            FloorballSaveEvent domainEvent, 
            CancellationToken cancellationToken = default)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId, cancellationToken);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for Save event.", domainEvent.MatchId);
                return (FloorballNotificationEvents.GoalieSave, null);
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";
            string savingTeamName = domainEvent.TeamId == match.HomeTeam?.Id ? homeTeamName : awayTeamName;

            // Create a FloorballSaveNotification object initializing all properties at once
            FloorballSaveNotification notification = new()
            {
                MatchId = domainEvent.MatchId,
                TeamId = domainEvent.TeamId,
                GoalieId = domainEvent.GoalieId,
                PeriodNumber = domainEvent.PeriodNumber,
                TimeInSeconds = domainEvent.TimeInSeconds,
                EventTime = domainEvent.OccurredOn,
                WasInOvertime = domainEvent.WasInOvertime,
                WasInShootout = domainEvent.WasInShootout,
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

            _logger.LogInformation("Save made by {TeamName} goalie in period {PeriodNumber} at {TimeInSeconds}s. Match: {HomeTeam} vs {AwayTeam}", 
                savingTeamName, domainEvent.PeriodNumber, domainEvent.TimeInSeconds, homeTeamName, awayTeamName);

            return (FloorballNotificationEvents.GoalieSave, notification);
        }
    }
} 
