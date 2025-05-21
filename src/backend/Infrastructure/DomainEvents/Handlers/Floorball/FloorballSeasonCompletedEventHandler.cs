using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballSeasonCompletedEvent by notifying SignalR clients with season details.
    /// </summary>
    public class FloorballSeasonCompletedEventHandler : SignalRDomainEventHandler<FloorballSeasonCompletedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonCompletedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonCompletedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballSeasonCompletedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballSeasonCompletedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballSeasonCompletedEvent domainEvent)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonCompleted event.", domainEvent.SeasonId);
                return;
            }

            object payload = new
            {
                SeasonId = season.Id,
                Name = season.Name,
                CompletedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season completed: {SeasonName}", season.Name);

            await NotifyAsync("FloorballSeasonCompleted", payload);
        }
    }
} 