using Domain.DomainEvents.Floorball;
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
    /// Handles FloorballTeamRemovedEvent by notifying SignalR clients that a team has been removed.
    /// </summary>
    public class FloorballTeamRemovedEventHandler : NotificationDomainEventHandler<FloorballTeamRemovedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballTeamRemovedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notificationSender">The notification sender</param>
        /// <param name="logger">The logger</param>
        public FloorballTeamRemovedEventHandler(
            ApplicationDbContext dbContext,
            INotificationSender notificationSender,
            ILogger<FloorballTeamRemovedEventHandler> logger)
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
        protected override Task<(string EventName, object? Notification)> BuildNotificationAsync(
            FloorballTeamRemovedEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            FloorballTeamRemovedNotification notification = new()
            {
                TeamId = domainEvent.TeamId,
                RemovedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Team removed: {TeamId}", domainEvent.TeamId);

            return Task.FromResult((FloorballNotificationEvents.TeamRemoved, (object?)notification));
        }
    }
} 