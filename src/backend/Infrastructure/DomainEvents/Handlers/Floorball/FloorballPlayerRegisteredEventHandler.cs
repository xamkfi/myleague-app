using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPlayerRegisteredEvent by notifying SignalR clients with player registration details.
    /// </summary>
    public class FloorballPlayerRegisteredEventHandler : SignalRDomainEventHandler<FloorballPlayerRegisteredEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerRegisteredEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerRegisteredEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballPlayerRegisteredEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballPlayerRegisteredEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballPlayerRegisteredEvent domainEvent)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerRegistered event.", domainEvent.PlayerId);
                return;
            }

            object payload = new
            {
                PlayerId = player.Id,
                PlayerName = player.Person?.FullName ?? "Unknown",
                Position = player.Position.ToString(),
                // JerseyNumber removed as it doesn't exist in FloorballPlayer
                PersonId = player.PersonId,
                RegistrationTime = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player registered: {PlayerName}", player.Person?.FullName ?? "Unknown");

            await NotifyAsync("FloorballPlayerRegistered", payload);
        }
    }
} 