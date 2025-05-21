// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// Handles FloorballSeasonDetailsUpdatedEvent by notifying SignalR clients with season detail update information.
    /// </summary>
    public class FloorballSeasonDetailsUpdatedEventHandler : NotificationDomainEventHandler<FloorballSeasonDetailsUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonDetailsUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonDetailsUpdatedEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballSeasonDetailsUpdatedEventHandler> logger)
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
            FloorballSeasonDetailsUpdatedEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId, cancellationToken);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonDetailsUpdated event.", domainEvent.SeasonId);
                return (FloorballNotificationEvents.SeasonDetailsUpdated, null);
            }

            FloorballSeasonDetailsUpdatedNotification notification = new()
            {
                SeasonId = season.Id,
                Name = domainEvent.Name,
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season details updated for season {SeasonId}: {Name}", season.Id, domainEvent.Name);

            return (FloorballNotificationEvents.SeasonDetailsUpdated, notification);
        }
    }
}
