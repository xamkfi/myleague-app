using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballGoalScoredEvent by notifying SignalR clients with goal details.
    /// </summary>
    public class FloorballGoalScoredEventHandler : SignalRDomainEventHandler<FloorballGoalScoredEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballGoalScoredEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballGoalScoredEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballGoalScoredEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballGoalScoredEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballGoalScoredEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for GoalScored event.", domainEvent.MatchId);
                return;
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";
            string scoringTeamName = domainEvent.TeamId == match.HomeTeam?.Id ? homeTeamName : awayTeamName;

            object payload = new
            {
                MatchId = domainEvent.MatchId,
                TeamId = domainEvent.TeamId,
                PlayerId = domainEvent.PlayerId,
                PeriodNumber = domainEvent.PeriodNumber,
                EventTime = domainEvent.OccurredOn,
                HomeTeam = new { Id = match.HomeTeam?.Id, Name = homeTeamName },
                AwayTeam = new { Id = match.AwayTeam?.Id, Name = awayTeamName }
            };

            _logger.LogInformation("Goal scored by {TeamName} in period {PeriodNumber}. Match: {HomeTeam} vs {AwayTeam}", 
                scoringTeamName, domainEvent.PeriodNumber, homeTeamName, awayTeamName);

            await NotifyAsync("FloorballGoalScored", payload);
        }
    }
} 