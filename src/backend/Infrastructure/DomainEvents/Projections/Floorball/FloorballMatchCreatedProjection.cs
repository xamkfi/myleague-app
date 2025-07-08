using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection for handling the FloorballMatch creation after event is stored.
    /// </summary>
    public sealed class FloorballMatchCreatedProjection : IDomainEventHandler<FloorballMatchCreatedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchCreatedProjection> _logger;

        public FloorballMatchCreatedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchCreatedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        /// <summary>
        /// Creating Floorball match after event storing
        /// </summary>
        public async Task HandleAsync(FloorballMatchCreatedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballMatchCreatedEvent for match {MatchId}", domainEvent.MatchId);

            bool exists = await _dbContext.FloorballMatches
                .AsNoTracking()
                .AnyAsync(m => m.Id == domainEvent.MatchId);
            if (exists)
            {
                _logger.LogDebug("Projection skipped – FloorballMatch {MatchId} already exists", domainEvent.MatchId);
                return;
            }

            try
            {
                FloorballSeason? season = await _dbContext.FloorballSeasons.FindAsync(domainEvent.SeasonId);
                FloorballTeam? homeTeam = await _dbContext.FloorballTeams.FindAsync(domainEvent.HomeTeamId);
                FloorballTeam? awayTeam = await _dbContext.FloorballTeams.FindAsync(domainEvent.AwayTeamId);

                if (season == null || homeTeam == null || awayTeam == null)
                {
                    _logger.LogWarning("Projection failed – required entities missing for match {MatchId}", domainEvent.MatchId);
                    return;
                }

                FloorballMatch match = new FloorballMatch(
                    domainEvent.MatchId,
                    season,
                    homeTeam,
                    awayTeam,
                    domainEvent.ScheduledDateTime,
                    domainEvent.Venue);

                _dbContext.FloorballMatches.Add(match);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Projection created FloorballMatch {MatchId}", domainEvent.MatchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when creating FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
}
