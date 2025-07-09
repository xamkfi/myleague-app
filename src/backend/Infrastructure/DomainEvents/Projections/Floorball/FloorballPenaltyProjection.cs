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
    /// Projection to handle adding of penalty event to Floorballmatch table.
    /// </summary>
    public class FloorballPenaltyProjection : IDomainEventHandler<FloorballPenaltyAssignedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballPenaltyProjection> _logger;

        public FloorballPenaltyProjection(FloorballDbContext dbContext, ILogger<FloorballPenaltyProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        /// <summary>
        /// Add the penalty after event storing.
        /// </summary>
        public async Task HandleAsync(FloorballPenaltyAssignedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballPenaltyAssignedEvent for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);

            try
            {
                FloorballPenalty penalty = new FloorballPenalty(
                    domainEvent.MatchId,
                    domainEvent.TeamId,
                    domainEvent.PlayerId,
                    domainEvent.PenaltyType,
                    domainEvent.Minutes,
                    domainEvent.PeriodNumber,
                    domainEvent.TimeInSeconds,
                    domainEvent.Description);

                _dbContext.FloorballPenalties.Add(penalty);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully added penalty for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when adding penalty for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);
            }
        }
    }
}
