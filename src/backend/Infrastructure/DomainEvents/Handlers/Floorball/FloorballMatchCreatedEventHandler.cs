using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballMatchCreatedEvent by notifying SignalR clients with new match details.
    /// </summary>
    public class FloorballMatchCreatedEventHandler : SignalRDomainEventHandler<FloorballMatchCreatedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballMatchCreatedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballMatchCreatedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballMatchCreatedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballMatchCreatedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballMatchCreatedEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Season)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for MatchCreated event.", domainEvent.MatchId);
                return;
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";
            string seasonName = match.Season?.Name ?? "Unknown";

            object payload = new
            {
                MatchId = match.Id,
                CreatedAt = domainEvent.OccurredOn,
                HomeTeam = new { Id = match.HomeTeam?.Id, Name = homeTeamName },
                AwayTeam = new { Id = match.AwayTeam?.Id, Name = awayTeamName },
                Season = new { Id = match.Season?.Id, Name = seasonName }
            };

            _logger.LogInformation("Match created: {HomeTeam} vs {AwayTeam}", homeTeamName, awayTeamName);

            await NotifyAsync("FloorballMatchCreated", payload);
        }
    }
} 