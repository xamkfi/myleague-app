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
    /// Projection for handling the change of a floorball match's season after the event is stored.
    /// </summary>
    public sealed class FloorballMatchSeasonChangedProjection : IDomainEventHandler<FloorballMatchSeasonChangedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchSeasonChangedProjection> _logger;

        public FloorballMatchSeasonChangedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchSeasonChangedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updates the match's season in the read model after the season change event is stored
        /// </summary>
        public async Task HandleAsync(FloorballMatchSeasonChangedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballMatchSeasonChangedEvent for match {MatchId}", domainEvent.MatchId);

            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Projection skipped – FloorballMatch {MatchId} not found", domainEvent.MatchId);
                return;
            }

            try
            {
                // Get the new season to ensure it exists
                FloorballSeason? newSeason = await _dbContext.FloorballSeasons.FindAsync(domainEvent.NewSeasonId);
                if (newSeason == null)
                {
                    _logger.LogWarning("Projection failed – FloorballSeason {SeasonId} not found", domainEvent.NewSeasonId);
                    return;
                }

                // Update the match's season
                match.ChangeSeason(newSeason);
                await _dbContext.SaveChangesWithoutEventsAsync();

                _logger.LogInformation("Projection updated FloorballMatch {MatchId} season from {PreviousSeasonId} to {NewSeasonId}", 
                    domainEvent.MatchId, domainEvent.PreviousSeasonId, domainEvent.NewSeasonId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when changing season for FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
} 