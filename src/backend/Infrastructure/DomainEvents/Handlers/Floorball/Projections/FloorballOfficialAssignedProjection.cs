using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball.Projections
{
    public class FloorballOfficialAssignedProjection : IDomainEventHandler<FloorballOfficialAssignedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballOfficialAssignedProjection> _logger;

        public FloorballOfficialAssignedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballOfficialAssignedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task HandleAsync(FloorballOfficialAssignedEvent domainEvent)
        {
            FloorballReferee? referee = await _dbContext.FloorballReferees.FindAsync(domainEvent.RefereeId);
            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);

            if (match is not null && referee is not null)
            {
                match.ProjectionAddReferee(referee);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Projected official assignment for match {MatchId}", domainEvent.MatchId);
            }
            else
            {
                _logger.LogWarning("Could not project official assignment. Match {MatchId} or Referee {RefereeId} not found.", domainEvent.MatchId, domainEvent.RefereeId);
            }
        }
    }
}
