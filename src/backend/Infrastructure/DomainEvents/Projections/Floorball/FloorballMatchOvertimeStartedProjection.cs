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
    /// Projection to handle a floorball match entering overtime.
    /// </summary>
    public sealed class FloorballMatchOvertimeStartedProjection : IDomainEventHandler<FloorballMatchOvertimeStartedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchOvertimeStartedProjection> _logger;

        public FloorballMatchOvertimeStartedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchOvertimeStartedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updating match status to overtime
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public async Task HandleAsync(FloorballMatchOvertimeStartedEvent domainEvent)
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
                match.RecordOvertime();

                // Ensure a period score row exists for overtime (period 4)
                bool hasOvertimePeriod = match.PeriodScores.Any(ps => ps.PeriodNumber == 4);
                if (!hasOvertimePeriod)
                {
                    FloorballPeriodScore overtimeScore = new FloorballPeriodScore(
                        match.Id,
                        4,
                        match.HomeTeamId,
                        match.AwayTeamId);

                    _dbContext.FloorballPeriodScores.Add(overtimeScore);
                }

                await _dbContext.SaveChangesWithoutEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when recording overtime for FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
} 
