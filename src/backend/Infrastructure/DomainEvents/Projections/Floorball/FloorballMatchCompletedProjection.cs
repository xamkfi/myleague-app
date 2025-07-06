using System;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection to update FloorballMatch status to Completed and sync final match data
    /// </summary>
    public class FloorballMatchCompletedProjection : IDomainEventHandler<FloorballMatchCompletedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchCompletedProjection> _logger;

        public FloorballMatchCompletedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchCompletedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updates the FloorballMatch read model with completion data after event storing
        /// </summary>
        /// <param name="domainEvent">The FloorballMatchCompletedEvent containing completion data</param>
        public async Task HandleAsync(FloorballMatchCompletedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballMatchCompletedEvent for match {MatchId}", domainEvent.MatchId);

            try
            {
                // Find the match in the read model
                FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
                
                if (match == null)
                {
                    _logger.LogWarning("FloorballMatch with ID {MatchId} not found for completion projection", domainEvent.MatchId);
                    return;
                }

                // Idempotency check: if already completed, skip processing
                if (match.Status == FloorballMatchStatus.Completed)
                {
                    _logger.LogDebug("Match {MatchId} already completed, skipping projection", domainEvent.MatchId);
                    return;
                }

                // Update match completion data
                match.Complete();
                
                // Update overtime/shootout flags from the event if they occurred
                if (domainEvent.WentToOvertime)
                {
                    match.RecordOvertime();
                }
                
                if (domainEvent.WentToShootout)
                {
                    match.RecordShootout();
                }

                // Save changes to database
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully completed match projection for {MatchId}. Final score: {HomeScore}-{AwayScore}, Overtime: {WentToOvertime}, Shootout: {WentToShootout}", 
                    domainEvent.MatchId, domainEvent.HomeScore, domainEvent.AwayScore, domainEvent.WentToOvertime, domainEvent.WentToShootout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process FloorballMatchCompletedEvent for match {MatchId}", domainEvent.MatchId);
                throw;
            }
        }
    }
} 