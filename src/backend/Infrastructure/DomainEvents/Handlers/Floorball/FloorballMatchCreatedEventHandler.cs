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
    /// Handles FloorballMatchCreatedEvent by notifying SignalR clients with new match details.
    /// </summary>
    public class FloorballMatchCreatedEventHandler : NotificationDomainEventHandler<FloorballMatchCreatedEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballMatchCreatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballMatchCreatedEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballMatchCreatedEventHandler> logger)
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
            FloorballMatchCreatedEvent domainEvent, 
            CancellationToken cancellationToken = default)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId, cancellationToken);
            
            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for MatchCreated event.", domainEvent.MatchId);
                return (FloorballNotificationEvents.MatchCreated, null);
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";

            // Create team info objects first
            TeamInfo homeTeamInfo = new TeamInfo
            {
                Id = match.HomeTeam?.Id ?? Guid.Empty,
                Name = homeTeamName
            };

            TeamInfo awayTeamInfo = new TeamInfo
            {
                Id = match.AwayTeam?.Id ?? Guid.Empty,
                Name = awayTeamName
            };

            FloorballMatchCreatedNotification notification = new FloorballMatchCreatedNotification
            {
                MatchId = match.Id,
                ScheduledDateTime = match.ScheduledDateTime,
                Location = match.Venue ?? string.Empty,
                HomeTeam = homeTeamInfo,
                AwayTeam = awayTeamInfo,
                CreatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Match created: {HomeTeam} vs {AwayTeam}", homeTeamName, awayTeamName);

            return (FloorballNotificationEvents.MatchCreated, notification);
        }
    }
} 
