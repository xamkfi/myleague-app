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
    /// Handles FloorballMatchAddedToSeasonEvent by notifying SignalR clients when a match is added to a season.
    /// </summary>
    public class FloorballMatchAddedToSeasonEventHandler : NotificationDomainEventHandler<FloorballMatchAddedToSeasonEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballMatchAddedToSeasonEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballMatchAddedToSeasonEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballMatchAddedToSeasonEventHandler> logger)
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
            FloorballMatchAddedToSeasonEvent domainEvent, 
            CancellationToken cancellationToken = default)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId, cancellationToken);

            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId, cancellationToken);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for MatchAddedToSeason event.", domainEvent.MatchId);
                return (FloorballNotificationEvents.MatchAddedToSeason, null);
            }

            string seasonName = season?.Name ?? "Unknown Season";
            string homeTeamName = match.HomeTeam?.Name ?? "Unknown Home Team";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown Away Team";

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

            FloorballMatchAddedToSeasonNotification notification = new FloorballMatchAddedToSeasonNotification
            {
                MatchId = match.Id,
                SeasonId = domainEvent.SeasonId,
                SeasonName = seasonName,
                ScheduledDateTime = match.ScheduledDateTime,
                HomeTeam = homeTeamInfo,
                AwayTeam = awayTeamInfo,
                AddedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Match {HomeTeam} vs {AwayTeam} added to season {SeasonName}", 
                homeTeamName, awayTeamName, seasonName);

            return (FloorballNotificationEvents.MatchAddedToSeason, notification);
        }
    }
} 