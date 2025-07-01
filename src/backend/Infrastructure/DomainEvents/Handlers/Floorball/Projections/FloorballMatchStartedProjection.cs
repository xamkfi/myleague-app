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

        public async Task HandleAsync(FloorballMatchStartedEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);

            if (match!=null)
            {
                match.ProjectionStart();
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Projected Start match {MatchId}", domainEvent.MatchId);
            }
            else
            {
                _logger.LogWarning("Could not start Match {MatchId}.", domainEvent.MatchId);
            }
        }
    }
}
