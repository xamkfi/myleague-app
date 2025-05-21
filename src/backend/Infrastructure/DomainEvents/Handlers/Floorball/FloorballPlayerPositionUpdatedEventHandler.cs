using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPlayerPositionUpdatedEvent by notifying SignalR clients with player position updates.
    /// </summary>
    public class FloorballPlayerPositionUpdatedEventHandler : SignalRDomainEventHandler<FloorballPlayerPositionUpdatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerPositionUpdatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerPositionUpdatedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballPlayerPositionUpdatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballPlayerPositionUpdatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballPlayerPositionUpdatedEvent domainEvent)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerPositionUpdated event.", domainEvent.PlayerId);
                return;
            }

            object payload = new
            {
                PlayerId = domainEvent.PlayerId,
                PlayerName = player.Person?.FullName ?? "Unknown",
                Position = domainEvent.Position.ToString(),
                UpdatedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player position updated for player {PlayerId}: {Position}", 
                domainEvent.PlayerId, domainEvent.Position);

            await NotifyAsync("FloorballPlayerPositionUpdated", payload);
        }
    }
} 