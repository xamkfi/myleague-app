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

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
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

        public async Task HandleAsync(FloorballOfficialAssignedEvent domainEvent)
        {
            FloorballMatch? match = await _floorballDbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
            FloorballReferee? referee = await _floorballDbContext.FloorballReferees.FindAsync(domainEvent.RefereeId);

            if (match == null || referee == null)
                return;

            match.AddOfficial(referee);
            await _floorballDbContext.SaveChangesAsync();
        }
    }
}
