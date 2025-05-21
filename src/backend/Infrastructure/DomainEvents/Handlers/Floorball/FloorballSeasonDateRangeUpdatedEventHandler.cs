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
    /// Handles FloorballSeasonDateRangeUpdatedEvent by notifying SignalR clients with date range update details.
    /// </summary>
    public class FloorballSeasonDateRangeUpdatedEventHandler : NotificationDomainEventHandler<FloorballSeasonDateRangeUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonDateRangeUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonDateRangeUpdatedEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballSeasonDateRangeUpdatedEventHandler> logger)
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
            FloorballSeasonDateRangeUpdatedEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId, cancellationToken);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonDateRangeUpdated event.", domainEvent.SeasonId);
                return (FloorballNotificationEvents.SeasonDateRangeUpdated, null);
            }

            FloorballSeasonDateRangeUpdatedNotification notification = new()
            {
                SeasonId = season.Id,
                StartDate = domainEvent.StartDate,
                EndDate = domainEvent.EndDate,
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season date range updated for season {SeasonId}: {StartDate} to {EndDate}", 
                season.Id, domainEvent.StartDate, domainEvent.EndDate);

            return (FloorballNotificationEvents.SeasonDateRangeUpdated, notification);
        }
    }
} 