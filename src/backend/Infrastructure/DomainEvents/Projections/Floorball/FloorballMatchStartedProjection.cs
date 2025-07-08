using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection to update Floorballmatch status to inProgress and create initial period scores
    /// </summary>
    public class FloorballMatchStartedProjection : IDomainEventHandler<FloorballMatchStartedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchStartedProjection> _logger;

        public FloorballMatchStartedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchStartedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        
        /// <summary>
        /// Starting the Floorball match after event storing and creating initial period scores
        /// </summary>
        public async Task HandleAsync(FloorballMatchStartedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballMatchStartedEvent for match {MatchId}", domainEvent.MatchId);
            
            try
            {
                FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
                if (match == null)
                {
                    _logger.LogWarning("FloorballMatch with ID {MatchId} not found for match started projection", domainEvent.MatchId);
                    return;
                }

                // Start the match
                match.Start();

                // Create initial period scores (1, 2, 3) with 0-0 scores
                for (int periodNumber = 1; periodNumber <= 3; periodNumber++)
                {
                    // Check if period score already exists
                    FloorballPeriodScore? existingPeriodScore = await _dbContext.FloorballPeriodScores
                        .FirstOrDefaultAsync(ps => ps.MatchId == domainEvent.MatchId && ps.PeriodNumber == periodNumber);

                    if (existingPeriodScore == null)
                    {
                        FloorballPeriodScore periodScore = new FloorballPeriodScore(
                            domainEvent.MatchId,
                            periodNumber,
                            match.HomeTeamId,
                            match.AwayTeamId);

                        _dbContext.FloorballPeriodScores.Add(periodScore);
                        
                        _logger.LogDebug("Created initial period score for match {MatchId}, period {PeriodNumber}", 
                            domainEvent.MatchId, periodNumber);
                    }
                }

                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Successfully started match {MatchId} and created initial period scores", domainEvent.MatchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process FloorballMatchStartedEvent for match {MatchId}", domainEvent.MatchId);
                throw;
            }
        }
    }
}
