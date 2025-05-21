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
    /// Handles FloorballSeasonDeactivatedEvent by notifying SignalR clients with season details.
    /// </summary>
    public class FloorballSeasonDeactivatedEventHandler : NotificationDomainEventHandler<FloorballSeasonDeactivatedEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonDeactivatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonDeactivatedEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballSeasonDeactivatedEventHandler> logger)
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
            FloorballSeasonDeactivatedEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId, cancellationToken);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonDeactivated event.", domainEvent.SeasonId);
                return (FloorballNotificationEvents.SeasonDeactivated, null);
            }

            FloorballSeasonDeactivatedNotification notification = new()
            {
                SeasonId = season.Id,
                Name = season.Name ?? "Unknown Season",
                DeactivatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season deactivated: {SeasonName}", season.Name);

            return (FloorballNotificationEvents.SeasonDeactivated, notification);
        }
    }
} 
