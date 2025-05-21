using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPlayerRemovedFromTeamEvent by notifying SignalR clients when a player is removed from a team.
    /// </summary>
    public class FloorballPlayerRemovedFromTeamEventHandler : SignalRDomainEventHandler<FloorballPlayerRemovedFromTeamEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerRemovedFromTeamEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerRemovedFromTeamEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballPlayerRemovedFromTeamEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballPlayerRemovedFromTeamEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballPlayerRemovedFromTeamEvent domainEvent)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId);

            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerRemovedFromTeam event.", domainEvent.PlayerId);
                return;
            }

            string teamName = team?.Name ?? "Unknown Team";

            object payload = new
            {
                PlayerId = domainEvent.PlayerId,
                TeamId = domainEvent.TeamId,
                PlayerName = player.Person?.FullName ?? "Unknown",
                TeamName = teamName,
                RemovedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player {PlayerName} removed from team {TeamName}", 
                player.Person?.FullName ?? "Unknown", teamName);

            await NotifyAsync("FloorballPlayerRemovedFromTeam", payload);
        }
    }
} 