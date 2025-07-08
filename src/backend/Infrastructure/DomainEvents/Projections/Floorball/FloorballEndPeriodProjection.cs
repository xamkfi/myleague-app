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
    /// Projection for updating period status to completed
    /// </summary>
    public sealed class FloorballEndPeriodProjection : IDomainEventHandler<FloorballPeriodEndedEvent>
    {
        private readonly FloorballDbContext _floorballDbContext;
        private readonly ILogger<FloorballEndPeriodProjection> _logger;

        public FloorballEndPeriodProjection(
            FloorballDbContext floorballDbContext,
            ILogger<FloorballEndPeriodProjection> logger)
        {
            _floorballDbContext = floorballDbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updating period completed to true
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public async Task HandleAsync(FloorballPeriodEndedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballPeriodEndedEvent for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);

            try
            {
                FloorballPeriodScore? period = await _floorballDbContext.FloorballPeriodScores
                    .FirstOrDefaultAsync(p => p.MatchId == domainEvent.MatchId && p.PeriodNumber == domainEvent.PeriodNumber);

                if (period == null)
                {
                    _logger.LogWarning("FloorballPeriodScore not found for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);
                    return;
                }

                if (period.IsCompleted)
                {
                    _logger.LogDebug("Period {PeriodNumber} for match {MatchId} already completed", domainEvent.PeriodNumber, domainEvent.MatchId);
                    return;
                }

                period.Complete();
                await _floorballDbContext.SaveChangesWithoutEventsAsync();

                _logger.LogInformation("Successfully completed period {PeriodNumber} for match {MatchId}", domainEvent.PeriodNumber, domainEvent.MatchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when completing period {PeriodNumber} for match {MatchId}", domainEvent.PeriodNumber, domainEvent.MatchId);
            }
        }
    }
}
