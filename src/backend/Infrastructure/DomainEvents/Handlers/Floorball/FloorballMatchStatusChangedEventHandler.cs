using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballMatchStatusChangedEvent by notifying SignalR clients with match status changes.
    /// </summary>
    public class FloorballMatchStatusChangedEventHandler : SignalRDomainEventHandler<FloorballMatchStatusChangedEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballMatchStatusChangedEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballMatchStatusChangedEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballMatchStatusChangedEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballMatchStatusChangedEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballMatchStatusChangedEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
            {
                _logger.LogWarning("Floorball match with ID {MatchId} not found for StatusChanged event.", domainEvent.MatchId);
                return;
            }

            string homeTeamName = match.HomeTeam?.Name ?? "Unknown";
            string awayTeamName = match.AwayTeam?.Name ?? "Unknown";

            object payload = new
            {
                MatchId = domainEvent.MatchId,
                ChangeTime = domainEvent.OccurredOn,
                PreviousStatus = domainEvent.PreviousStatus,
                NewStatus = domainEvent.NewStatus,
                HomeTeam = new { Id = match.HomeTeam?.Id, Name = homeTeamName },
                AwayTeam = new { Id = match.AwayTeam?.Id, Name = awayTeamName }
            };

            _logger.LogInformation("Match status changed from {OldStatus} to {NewStatus} for match between {HomeTeam} and {AwayTeam}", 
                domainEvent.PreviousStatus, domainEvent.NewStatus, homeTeamName, awayTeamName);

            await NotifyAsync("FloorballMatchStatusChanged", payload);
        }
    }
} 