using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballPlayerAddedToTeamEvent by notifying SignalR clients when a player is added to a team.
    /// </summary>
    public class FloorballPlayerAddedToTeamEventHandler : SignalRDomainEventHandler<FloorballPlayerAddedToTeamEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballPlayerAddedToTeamEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballPlayerAddedToTeamEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballPlayerAddedToTeamEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballPlayerAddedToTeamEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballPlayerAddedToTeamEvent domainEvent)
        {
            FloorballPlayer? player = await _dbContext.FloorballPlayers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(p => p.Id == domainEvent.PlayerId);

            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId);

            if (player == null)
            {
                _logger.LogWarning("Floorball player with ID {PlayerId} not found for PlayerAddedToTeam event.", domainEvent.PlayerId);
                return;
            }

            string teamName = team?.Name ?? "Unknown Team";

            object payload = new
            {
                PlayerId = domainEvent.PlayerId,
                TeamId = domainEvent.TeamId,
                PlayerName = player.Person?.FullName ?? "Unknown",
                TeamName = teamName,
                JerseyNumber = domainEvent.JerseyNumber,
                Position = domainEvent.Position.ToString(),
                AddedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Player {PlayerName} added to team {TeamName} with jersey #{JerseyNumber}", 
                player.Person?.FullName ?? "Unknown", teamName, domainEvent.JerseyNumber);

            await NotifyAsync("FloorballPlayerAddedToTeam", payload);
        }
    }
} 