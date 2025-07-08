using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Adding referee to a match after event storing
    /// </summary>
    public sealed class FloorballMatchAddOfficialProjection : IDomainEventHandler<FloorballOfficialAssignedEvent>
    {
        private readonly FloorballDbContext _floorballDbContext;
        private readonly ILogger<FloorballMatchAddOfficialProjection> _logger;

        public FloorballMatchAddOfficialProjection(
            FloorballDbContext floorballDbContext,
            ILogger<FloorballMatchAddOfficialProjection> logger)
        {
            _floorballDbContext = floorballDbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adding referee to a match.
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public async Task HandleAsync(FloorballOfficialAssignedEvent domainEvent)
        {

            FloorballMatch? match = await _floorballDbContext.FloorballMatches
                .Include(m => m.Officials)
                .FirstOrDefaultAsync(m => m.Id == domainEvent.MatchId);

            FloorballReferee? referee = await _floorballDbContext.FloorballReferees
                .FirstOrDefaultAsync(m => m.Id == domainEvent.RefereeId);

            if (match == null || referee == null)
                return;

            if (match.Officials.Any(o => o.Id == referee.Id))
                return;

            match.AddOfficial(referee);
            await _floorballDbContext.SaveChangesWithoutEventsAsync();
        }
    }
}
