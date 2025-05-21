using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballSeasonDateRangeUpdatedEvent by notifying SignalR clients with date range update details.
    /// </summary>
    public class FloorballSeasonDateRangeUpdatedEventHandler : SignalRDomainEventHandler<FloorballSeasonDateRangeUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonDateRangeUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonDateRangeUpdatedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballSeasonDateRangeUpdatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballSeasonDateRangeUpdatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballSeasonDateRangeUpdatedEvent domainEvent)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonDateRangeUpdated event.", domainEvent.SeasonId);
                return;
            }

            object payload = new
            {
                SeasonId = season.Id,
                StartDate = domainEvent.StartDate,
                EndDate = domainEvent.EndDate,
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season date range updated for season {SeasonId}: {StartDate} to {EndDate}", 
                season.Id, domainEvent.StartDate, domainEvent.EndDate);

            await NotifyAsync("FloorballSeasonDateRangeUpdated", payload);
        }
    }
} 