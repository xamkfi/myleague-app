using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection to update Floorballmatch status to inProgress
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
        /// Starting the Floorball match after event storing
        /// </summary>
        public async Task HandleAsync(FloorballMatchStartedEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
            if (match == null)
                return;
            match.Start();
            await _dbContext.SaveChangesAsync();
        }
    }
}
