using Domain.DomainEvents.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballTeamRemovedEvent by notifying SignalR clients that a team has been removed.
    /// </summary>
    public class FloorballTeamRemovedEventHandler : SignalRDomainEventHandler<FloorballTeamRemovedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballTeamRemovedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballTeamRemovedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballTeamRemovedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballTeamRemovedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballTeamRemovedEvent domainEvent)
        {
            object payload = new
            {
                TeamId = domainEvent.TeamId,
                RemovedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Team removed: {TeamId}", domainEvent.TeamId);

            await NotifyAsync("FloorballTeamRemoved", payload);
        }
    }
} 