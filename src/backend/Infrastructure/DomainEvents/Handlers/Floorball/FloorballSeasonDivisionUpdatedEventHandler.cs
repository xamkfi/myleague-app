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
    /// Handles FloorballSeasonDivisionUpdatedEvent by notifying SignalR clients with division update details.
    /// </summary>
    public class FloorballSeasonDivisionUpdatedEventHandler : NotificationDomainEventHandler<FloorballSeasonDivisionUpdatedEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonDivisionUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonDivisionUpdatedEventHandler(
            FloorballDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballSeasonDivisionUpdatedEventHandler> logger)
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
            FloorballSeasonDivisionUpdatedEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId, cancellationToken);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonDivisionUpdated event.", domainEvent.SeasonId);
                return (FloorballNotificationEvents.SeasonDivisionUpdated, null);
            }

            FloorballSeasonDivisionUpdatedNotification notification = new()
            {
                SeasonId = season.Id,
                DivisionId = domainEvent.Division.Id,
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season division updated for season {SeasonId}: {DivisionId}", season.Id, domainEvent.Division.Id);

            return (FloorballNotificationEvents.SeasonDivisionUpdated, notification);
        }
    }
} 

