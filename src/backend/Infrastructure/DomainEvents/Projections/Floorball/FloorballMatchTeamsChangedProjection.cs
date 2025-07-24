using System;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection for handling the change of a floorball match's teams after the event is stored.
    /// </summary>
    public sealed class FloorballMatchTeamsChangedProjection : IDomainEventHandler<FloorballMatchTeamsChangedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchTeamsChangedProjection> _logger;

        public FloorballMatchTeamsChangedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchTeamsChangedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updates the match's teams in the read model after the teams change event is stored
        /// </summary>
        public async Task HandleAsync(FloorballMatchTeamsChangedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballMatchTeamsChangedEvent for match {MatchId}", domainEvent.MatchId);

            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Projection skipped – FloorballMatch {MatchId} not found", domainEvent.MatchId);
                return;
            }

            try
            {
                // Get the new teams to ensure they exist
                FloorballTeam? newHomeTeam = await _dbContext.FloorballTeams.FindAsync(domainEvent.NewHomeTeamId);
                FloorballTeam? newAwayTeam = await _dbContext.FloorballTeams.FindAsync(domainEvent.NewAwayTeamId);
                
                if (newHomeTeam == null)
                {
                    _logger.LogWarning("Projection failed – FloorballTeam {TeamId} not found", domainEvent.NewHomeTeamId);
                    return;
                }
                
                if (newAwayTeam == null)
                {
                    _logger.LogWarning("Projection failed – FloorballTeam {TeamId} not found", domainEvent.NewAwayTeamId);
                    return;
                }

                // Update the match's teams
                match.ChangeTeams(newHomeTeam, newAwayTeam);
                await _dbContext.SaveChangesWithoutEventsAsync();

                _logger.LogInformation("Projection updated FloorballMatch {MatchId} teams from {PreviousHomeTeamId}/{PreviousAwayTeamId} to {NewHomeTeamId}/{NewAwayTeamId}", 
                    domainEvent.MatchId, domainEvent.PreviousHomeTeamId, domainEvent.PreviousAwayTeamId, domainEvent.NewHomeTeamId, domainEvent.NewAwayTeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when changing teams for FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
} 