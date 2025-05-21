using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.SignalR;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball
{
    /// <summary>
    /// Handles FloorballTeamRemovedFromSeasonEvent by notifying SignalR clients with team and season details.
    /// </summary>
    public class FloorballTeamRemovedFromSeasonEventHandler : SignalRDomainEventHandler<FloorballTeamRemovedFromSeasonEvent>
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the FloorballTeamRemovedFromSeasonEventHandler class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="notifier">The domain event notifier</param>
        /// <param name="logger">The logger</param>
        public FloorballTeamRemovedFromSeasonEventHandler(
            ApplicationDbContext dbContext,
            DomainEventNotifier notifier,
            ILogger<FloorballTeamRemovedFromSeasonEventHandler> logger)
            : base(notifier, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Processes the FloorballTeamRemovedFromSeasonEvent before notification
        /// </summary>
        /// <param name="domainEvent">The domain event to process</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ProcessEventAsync(FloorballTeamRemovedFromSeasonEvent domainEvent)
        {
            FloorballSeason? season = await _dbContext.FloorballSeasons
                .FirstOrDefaultAsync(s => s.Id == domainEvent.SeasonId);

            FloorballTeam? team = await _dbContext.FloorballTeams
                .FirstOrDefaultAsync(t => t.Id == domainEvent.TeamId);

            if (season == null)
            {
                _logger.LogWarning("Floorball season with ID {SeasonId} not found for TeamRemovedFromSeason event.", domainEvent.SeasonId);
                return;
            }

            if (team == null)
            {
                _logger.LogWarning("Floorball team with ID {TeamId} not found for TeamRemovedFromSeason event.", domainEvent.TeamId);
                return;
            }

            object payload = new
            {
                SeasonId = season.Id,
                SeasonName = season.Name,
                TeamId = team.Id,
                TeamName = team.Name,
                RemovedOn = domainEvent.OccurredOn
            };

            _logger.LogInformation("Team {TeamName} removed from season {SeasonName}", team.Name, season.Name);

            await NotifyAsync("FloorballTeamRemovedFromSeason", payload);
        }
    }
} 