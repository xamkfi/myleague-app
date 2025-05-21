using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballMatchStartedEvent by notifying SignalR clients with match details.
    /// </summary>
    public class FloorballMatchStartedEventHandler : SignalRDomainEventHandler<FloorballMatchStartedEvent>
    {
        private readonly FloorballDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballMatchStartedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballMatchStartedEventHandler(
            FloorballDbContext dbContext,
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
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for MatchStarted event.", domainEvent.MatchId);
                return;
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";

            object payload = new
            {
                MatchId = match.Id,
                StartTime = domainEvent.OccurredOn,
                HomeTeam = new { match.HomeTeam?.Id, Name = homeTeamName },
                AwayTeam = new { match.AwayTeam?.Id, Name = awayTeamName }
            };

            _logger.LogInformation("Match started: {HomeTeam} vs {AwayTeam}", homeTeamName, awayTeamName);

            await NotifyAsync("FloorballMatchStarted", payload);
        }
    }
} 