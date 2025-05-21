using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPlayerStatUpdatedEvent by notifying SignalR clients with player stat updates.
    /// </summary>
    public class FloorballPlayerStatUpdatedEventHandler : SignalRDomainEventHandler<FloorballPlayerStatUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerStatUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerStatUpdatedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballPlayerStatUpdatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballPlayerStatUpdatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballPlayerStatUpdatedEvent domainEvent)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerStatUpdated event.", domainEvent.PlayerId);
                return;
            }

            object payload = new
            {
                PlayerId = domainEvent.PlayerId,
                PlayerName = player.Person?.FullName ?? "Unknown",
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player stats updated for player {PlayerId}", domainEvent.PlayerId);

            await NotifyAsync("FloorballPlayerStatUpdated", payload);
        }
    }
} 