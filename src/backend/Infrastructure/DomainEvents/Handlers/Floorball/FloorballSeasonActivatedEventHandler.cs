using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;
using System.Threading.Tasks;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballSeasonActivatedEvent by notifying SignalR clients when a season is activated.
    /// </summary>
    public class FloorballSeasonActivatedEventHandler : SignalRDomainEventHandler<FloorballSeasonActivatedEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballSeasonActivatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballSeasonActivatedEventHandler(
            FloorballDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballSeasonActivatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballSeasonActivatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballSeasonActivatedEvent domainEvent)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for SeasonActivated event.", domainEvent.SeasonId);
                return;
            }

            object payload = new
            {
                SeasonId = season.Id,
                Name = season.Name ?? "Unknown Season",
                ActivatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Season activated: {SeasonName}", season.Name);

            await NotifyAsync("SeasonActivated", payload);
        }
    }
} 
