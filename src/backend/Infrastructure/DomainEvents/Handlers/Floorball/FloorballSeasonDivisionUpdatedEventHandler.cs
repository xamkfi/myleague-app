using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballSeasonDivisionUpdatedEvent by notifying SignalR clients with division update details.
    /// </summary>
    public class FloorballSeasonDivisionUpdatedEventHandler : SignalRDomainEventHandler<FloorballSeasonDivisionUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonDivisionUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonDivisionUpdatedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballSeasonDivisionUpdatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballSeasonDivisionUpdatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballSeasonDivisionUpdatedEvent domainEvent)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonDivisionUpdated event.", domainEvent.SeasonId);
                return;
            }

            object payload = new
            {
                SeasonId = season.Id,
                Division = domainEvent.Division,
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season division updated for season {SeasonId}: {Division}", season.Id, domainEvent.Division);

            await NotifyAsync("FloorballSeasonDivisionUpdated", payload);
        }
    }
} 