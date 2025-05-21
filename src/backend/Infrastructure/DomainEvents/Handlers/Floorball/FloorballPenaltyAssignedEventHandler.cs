using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPenaltyAssignedEvent by notifying SignalR clients with penalty details.
    /// </summary>
    public class FloorballPenaltyAssignedEventHandler : SignalRDomainEventHandler<FloorballPenaltyAssignedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPenaltyAssignedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballPenaltyAssignedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballPenaltyAssignedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballPenaltyAssignedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballPenaltyAssignedEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for PenaltyAssigned event.", domainEvent.MatchId);
                return;
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";

            object payload = new
            {
                MatchId = domainEvent.MatchId,
                EventTime = domainEvent.OccurredOn,
                PenaltyType = domainEvent.PenaltyType,
                TeamId = domainEvent.TeamId,
                PlayerId = domainEvent.PlayerId,
                HomeTeam = new { Id = match.HomeTeam?.Id, Name = homeTeamName },
                AwayTeam = new { Id = match.AwayTeam?.Id, Name = awayTeamName }
            };

            _logger.LogInformation("Penalty assigned in match between {HomeTeam} and {AwayTeam}", 
                homeTeamName, awayTeamName);

            await NotifyAsync("FloorballPenaltyAssigned", payload);
        }
    }
} 