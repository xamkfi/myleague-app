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
            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);

            FloorballPeriodScore? periodScore = await _dbContext.FloorballPeriodScores
                .FirstOrDefaultAsync(x => x.MatchId == domainEvent.MatchId && x.PeriodNumber == domainEvent.PeriodNumber);

            FloorballGoal goal = new FloorballGoal(
                domainEvent.MatchId,
                domainEvent.TeamId,
                domainEvent.PlayerId,
                domainEvent.AssisterId,
                domainEvent.PeriodNumber,
                domainEvent.TimeInSeconds);

            if (match == null || periodScore == null)
                return;

            if (domainEvent.TeamId == match.HomeTeamId)
                periodScore.IncrementHomeScore();
            else
                periodScore.IncrementAwayScore();


            match.UpdateScore(domainEvent.TeamId);
            _dbContext.FloorballGoals.Add(goal);

            await _dbContext.SaveChangesAsync();
        }
    }
}
