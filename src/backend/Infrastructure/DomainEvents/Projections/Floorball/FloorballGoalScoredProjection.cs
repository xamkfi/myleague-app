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
    /// Projection for updating a goal count
    /// </summary>
    class FloorballGoalScoredProjection : IDomainEventHandler<FloorballGoalScoredEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballGoalScoredProjection> _logger;

        public FloorballGoalScoredProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballGoalScoredProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updating goal count after event storing
        /// </summary>
        public async Task HandleAsync(FloorballGoalScoredEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballGoalScoredEvent for match {MatchId}, period {PeriodNumber}", 
                domainEvent.MatchId, domainEvent.PeriodNumber);

            try
            {
                FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);

                if (match == null)
                {
                    _logger.LogWarning("FloorballMatch with ID {MatchId} not found for goal scoring projection", domainEvent.MatchId);
                    return;
                }

                FloorballPeriodScore? periodScore = await _dbContext.FloorballPeriodScores
                    .FirstOrDefaultAsync(x => x.MatchId == domainEvent.MatchId && x.PeriodNumber == domainEvent.PeriodNumber);

                // Create period score if it doesn't exist
                if (periodScore == null)
                {
                    _logger.LogInformation("Creating missing FloorballPeriodScore for match {MatchId}, period {PeriodNumber}", 
                        domainEvent.MatchId, domainEvent.PeriodNumber);

                    periodScore = new FloorballPeriodScore(
                        domainEvent.MatchId,
                        domainEvent.PeriodNumber,
                        match.HomeTeamId,
                        match.AwayTeamId);

                    _dbContext.FloorballPeriodScores.Add(periodScore);
                }

                // Update period score
                if (domainEvent.TeamId == match.HomeTeamId)
                {
                    periodScore.IncrementHomeScore();
                    _logger.LogDebug("Incremented home score for period {PeriodNumber} to {HomeScore}", 
                        domainEvent.PeriodNumber, periodScore.HomeScore);
                }
                else
                {
                    periodScore.IncrementAwayScore();
                    _logger.LogDebug("Incremented away score for period {PeriodNumber} to {AwayScore}", 
                        domainEvent.PeriodNumber, periodScore.AwayScore);
                }

                // Update match total score
                match.UpdateScore(domainEvent.TeamId);

                // Create goal record
                FloorballGoal goal = new FloorballGoal(
                    domainEvent.MatchId,
                    domainEvent.TeamId,
                    domainEvent.PlayerId,
                    domainEvent.AssisterId,
                    domainEvent.SecondaryAssisterId,
                    domainEvent.PeriodNumber,
                    domainEvent.TimeInSeconds);

                _dbContext.FloorballGoals.Add(goal);

                // Save all changes
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully processed goal for match {MatchId}, period {PeriodNumber}. Match score: {HomeScore}-{AwayScore}", 
                    domainEvent.MatchId, domainEvent.PeriodNumber, match.HomeScore, match.AwayScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process FloorballGoalScoredEvent for match {MatchId}, period {PeriodNumber}", 
                    domainEvent.MatchId, domainEvent.PeriodNumber);
                throw;
            }
        }
    }
}
