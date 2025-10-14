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
using Application.Interfaces.Common;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPlayerAddedToTeamEvent by notifying SignalR clients when a player is added to a team.
    /// </summary>
    public class FloorballPlayerAddedToTeamEventHandler : NotificationDomainEventHandler<FloorballPlayerAddedToTeamEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly IPersonNameProvider _personNameProvider;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerAddedToTeamEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerAddedToTeamEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballPlayerAddedToTeamEventHandler> logger,
            IPersonNameProvider personNameProvider)
            : base(notificationSender, logger)
        {
            _dbContext = dbContext;
            _personNameProvider = personNameProvider;
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
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId, cancellationToken);

            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId, cancellationToken);

            if (player is not FloorballPlayer p || team is not FloorballTeam t)
            {
                _logger.LogWarning(
                    "Floorball player with ID {PlayerId} or team with ID {TeamId} not found for PlayerAddedToTeam event.",
                    domainEvent.PlayerId, domainEvent.TeamId);
                return (FloorballNotificationEvents.PlayerAddedToTeam, null);
            }

            string playerName = await _personNameProvider.GetFullNameAsync(p.PersonId, cancellationToken);
            string teamName = t.Name ?? "Unknown Team";

            FloorballPlayerAddedToTeamNotification notification = new()
            {
                PlayerId = p.Id,
                TeamId = t.Id,
                PlayerName = playerName,
                TeamName = teamName,
                AddedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player {PlayerName} added to team {TeamName}", playerName, teamName);

            return (FloorballNotificationEvents.PlayerAddedToTeam, notification);
        }
    }
} 
