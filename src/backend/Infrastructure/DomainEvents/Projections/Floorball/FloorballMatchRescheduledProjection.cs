using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection for handling rescheduling of a floorball match. Updates the scheduled
    /// date, time and venue in the write model after the corresponding domain event is stored.
    /// </summary>
    public sealed class FloorballMatchRescheduledProjection : IDomainEventHandler<FloorballMatchRescheduledEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchRescheduledProjection> _logger;

        public FloorballMatchRescheduledProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchRescheduledProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Handles the <see cref="FloorballMatchRescheduledEvent"/> by updating the persisted match.
        /// </summary>
        public async Task HandleAsync(FloorballMatchRescheduledEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Projection skipped – FloorballMatch {MatchId} not found", domainEvent.MatchId);
                return;
            }

            try
            {
                // Use domain logic (adds another event) but persist without dispatching further events
                match.Reschedule(domainEvent.NewScheduledDateTime, domainEvent.NewVenue);
                await _dbContext.SaveChangesWithoutEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed while rescheduling FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
} 