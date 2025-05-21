using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers
{
    /// <summary>
    /// Handler for the FloorballMatchStartedEvent
    /// </summary>
    public class FloorballMatchStartedEventHandler : SignalRDomainEventHandler<FloorballMatchStartedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballMatchStartedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballMatchStartedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballMatchStartedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballMatchStartedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballMatchStartedEvent domainEvent)
        {
            // Retrieve additional data for the notification
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
            {
                return;
            }

            // Log additional information about the match that started
            // This could be used for analytics or to enrich the notification
            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";
            
            // Additional processing logic can be added here
            // For example, updating statistics, sending notifications to team followers, etc.
        }
    }
} 