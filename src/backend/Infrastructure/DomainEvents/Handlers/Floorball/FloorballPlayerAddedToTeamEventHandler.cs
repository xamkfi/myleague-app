using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.DTOs.Notifications;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;
using System.Threading;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPlayerAddedToTeamEvent by notifying SignalR clients when a player is added to a team.
    /// </summary>
    public class FloorballPlayerAddedToTeamEventHandler : NotificationDomainEventHandler<FloorballPlayerAddedToTeamEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerAddedToTeamEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerAddedToTeamEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballPlayerAddedToTeamEventHandler> logger)
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
            FloorballPlayerAddedToTeamEvent domainEvent, 
            CancellationToken cancellationToken = default)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId, cancellationToken);

            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId, cancellationToken);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerAddedToTeam event.", domainEvent.PlayerId);
                return ("FloorballPlayerAddedToTeam", null);
            }

            string teamName = team?.Name ?? "Unknown Team";
            string playerName = player.Person?.FullName ?? "Unknown";

            FloorballPlayerAddedToTeamNotification notification = new FloorballPlayerAddedToTeamNotification
            {
                PlayerId = domainEvent.PlayerId,
                TeamId = domainEvent.TeamId,
                PlayerName = playerName,
                TeamName = teamName,
                JerseyNumber = domainEvent.JerseyNumber,
                Position = domainEvent.Position.ToString(),
                AddedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player {PlayerName} added to team {TeamName} with jersey #{JerseyNumber}", 
                playerName, teamName, domainEvent.JerseyNumber);

            return ("FloorballPlayerAddedToTeam", notification);
        }
    }
} 