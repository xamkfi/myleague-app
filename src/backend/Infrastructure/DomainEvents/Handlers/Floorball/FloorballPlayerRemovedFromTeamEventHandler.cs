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
    /// Handles FloorballPlayerRemovedFromTeamEvent by notifying SignalR clients when a player is removed from a team.
    /// </summary>
    public class FloorballPlayerRemovedFromTeamEventHandler : NotificationDomainEventHandler<FloorballPlayerRemovedFromTeamEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerRemovedFromTeamEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerRemovedFromTeamEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballPlayerRemovedFromTeamEventHandler> logger)
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
            FloorballPlayerRemovedFromTeamEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId, cancellationToken);

            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId, cancellationToken);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerRemovedFromTeam event.", domainEvent.PlayerId);
                return (FloorballNotificationEvents.PlayerRemovedFromTeam, null);
            }

            string teamName = team?.Name ?? "Unknown Team";

            FloorballPlayerRemovedFromTeamNotification notification = new()
            {
                PlayerId = domainEvent.PlayerId,
                TeamId = domainEvent.TeamId,
                PlayerName = player.Person?.FullName ?? "Unknown",
                TeamName = teamName,
                RemovedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player {PlayerName} removed from team {TeamName}", 
                player.Person?.FullName ?? "Unknown", teamName);

            return (FloorballNotificationEvents.PlayerRemovedFromTeam, notification);
        }
    }
} 