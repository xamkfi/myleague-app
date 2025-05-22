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
    /// Handles FloorballPlayerStatUpdatedEvent by notifying SignalR clients with player stat updates.
    /// </summary>
    public class FloorballPlayerStatUpdatedEventHandler : NotificationDomainEventHandler<FloorballPlayerStatUpdatedEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerStatUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerStatUpdatedEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballPlayerStatUpdatedEventHandler> logger)
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
            FloorballPlayerStatUpdatedEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId, cancellationToken);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerStatUpdated event.", domainEvent.PlayerId);
                return (FloorballNotificationEvents.PlayerStatUpdated, null);
            }

            FloorballPlayerStatUpdatedNotification notification = new()
            {
                PlayerId = domainEvent.PlayerId,
                PlayerName = player.Person?.FullName ?? "Unknown",
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player stats updated for player {PlayerId}", domainEvent.PlayerId);

            return (FloorballNotificationEvents.PlayerStatUpdated, notification);
        }
    }
} 
