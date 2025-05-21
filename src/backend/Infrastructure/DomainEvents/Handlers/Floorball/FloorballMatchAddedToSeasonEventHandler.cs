using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballMatchAddedToSeasonEvent by notifying SignalR clients when a match is added to a season.
    /// </summary>
    public class FloorballMatchAddedToSeasonEventHandler : SignalRDomainEventHandler<FloorballMatchAddedToSeasonEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballMatchAddedToSeasonEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballMatchAddedToSeasonEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballMatchAddedToSeasonEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballMatchAddedToSeasonEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballMatchAddedToSeasonEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for MatchAddedToSeason event.", domainEvent.MatchId);
                return;
            }

            string seasonName = season?.Name ?? "Unknown Season";
            string homeTeamName = match.HomeTeam?.Name ?? "Unknown Home Team";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown Away Team";

            object payload = new
            {
                MatchId = match.Id,
                SeasonId = domainEvent.SeasonId,
                HomeTeamId = match.HomeTeamId,
                AwayTeamId = match.AwayTeamId,
                HomeTeamName = homeTeamName,
                AwayTeamName = awayTeamName,
                // ScheduledTime removed as it doesn't exist in FloorballMatch
                Venue = match.Venue,
                SeasonName = seasonName,
                AddedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Match {HomeTeam} vs {AwayTeam} added to season {SeasonName}", 
                homeTeamName, awayTeamName, seasonName);

            await NotifyAsync("FloorballMatchAddedToSeason", payload);
        }
    }
} 