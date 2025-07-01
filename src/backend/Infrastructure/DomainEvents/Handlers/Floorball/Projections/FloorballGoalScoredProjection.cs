using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball.Projections
{
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

        public async Task HandleAsync(FloorballGoalScoredEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);

            if (match == null)
                return;

            match.ProjectionRecordGoal(domainEvent.TeamId, domainEvent.PlayerId, domainEvent.PeriodNumber, domainEvent.TimeInSeconds, domainEvent.AssisterId);
            await _dbContext.SaveChangesAsync();
        }
    }
}
