using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection to handle a floorball match entering shootout phase.
    /// </summary>
    public sealed class FloorballMatchShootoutStartedProjection : IDomainEventHandler<FloorballMatchShootoutStartedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchShootoutStartedProjection> _logger;

        public FloorballMatchShootoutStartedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchShootoutStartedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updating match status to shootouts 
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public async Task HandleAsync(FloorballMatchShootoutStartedEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches
                .Include(m => m.PeriodScores)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            if (match == null)
            {
                _logger.LogWarning("Projection skipped – FloorballMatch {MatchId} not found", domainEvent.MatchId);
                return;
            }

            try
            {
                match.RecordShootout();

                // Ensure a period score row exists for shootout (period 5)
                bool hasShootoutPeriod = match.PeriodScores.Any(ps => ps.PeriodNumber == 5);
                if (!hasShootoutPeriod)
                {
                    FloorballPeriodScore shootoutScore = new FloorballPeriodScore(
                        match.Id,
                        5,
                        match.HomeTeamId,
                        match.AwayTeamId);

                    _dbContext.FloorballPeriodScores.Add(shootoutScore);
                }

                await _dbContext.SaveChangesWithoutEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when recording shootout for FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
} 
