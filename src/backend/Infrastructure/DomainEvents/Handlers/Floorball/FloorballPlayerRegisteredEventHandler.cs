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
    /// Handles FloorballPlayerRegisteredEvent by notifying SignalR clients with player registration details.
    /// </summary>
    public class FloorballPlayerRegisteredEventHandler : NotificationDomainEventHandler<FloorballPlayerRegisteredEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly IPersonNameProvider _personNameProvider;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerRegisteredEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerRegisteredEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballPlayerRegisteredEventHandler> logger,
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
            FloorballPlayerRegisteredEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId, cancellationToken);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerRegistered event.", domainEvent.PlayerId);
                return (FloorballNotificationEvents.PlayerRegistered, null);
            }

            string playerName = player != null
                ? await _personNameProvider.GetFullNameAsync(player.PersonId, cancellationToken)
                : "Unknown";

            FloorballPlayerRegisteredNotification notification = new()
            {
                PlayerId = player.Id,
                PlayerName = playerName,
                Position = player.Position?.ToString() ?? "Unknown",
                PersonId = player.PersonId,
                RegistrationTime = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player registered: {PlayerName}", playerName);

            return (FloorballNotificationEvents.PlayerRegistered, notification);
        }
    }
} 

