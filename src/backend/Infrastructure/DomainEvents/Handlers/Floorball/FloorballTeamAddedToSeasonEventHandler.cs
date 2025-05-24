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
    /// Handles FloorballTeamAddedToSeasonEvent by notifying SignalR clients with team and season details.
    /// </summary>
    public class FloorballTeamAddedToSeasonEventHandler : NotificationDomainEventHandler<FloorballTeamAddedToSeasonEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballTeamAddedToSeasonEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballTeamAddedToSeasonEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballTeamAddedToSeasonEventHandler> logger)
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
            FloorballTeamAddedToSeasonEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId, cancellationToken);

            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId, cancellationToken);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for TeamAddedToSeason event.", domainEvent.SeasonId);
                return (FloorballNotificationEvents.TeamAddedToSeason, null);
            }

            if (team == null)
            {
                _logger.LogWarning("Floorball team with ID {TeamId} not found for TeamAddedToSeason event.", domainEvent.TeamId);
                return (FloorballNotificationEvents.TeamAddedToSeason, null);
            }

            FloorballTeamAddedToSeasonNotification notification = new()
            {
                SeasonId = season.Id,
                SeasonName = season.Name ?? "Unknown Season",
                TeamId = team.Id,
                TeamName = team.Name ?? "Unknown Team",
                AddedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Team {TeamName} added to season {SeasonName}", team.Name, season.Name);

            return (FloorballNotificationEvents.TeamAddedToSeason, notification);
        }
    }
} 
