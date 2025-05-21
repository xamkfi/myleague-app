using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballSeasonDeactivatedEvent by notifying SignalR clients with season details.
    /// </summary>
    public class FloorballSeasonDeactivatedEventHandler : SignalRDomainEventHandler<FloorballSeasonDeactivatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonDeactivatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonDeactivatedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballSeasonDeactivatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballSeasonDeactivatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballSeasonDeactivatedEvent domainEvent)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonDeactivated event.", domainEvent.SeasonId);
                return;
            }

            object payload = new
            {
                SeasonId = season.Id,
                Name = season.Name,
                DeactivatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season deactivated: {SeasonName}", season.Name);

            await NotifyAsync("FloorballSeasonDeactivated", payload);
        }
    }
} 