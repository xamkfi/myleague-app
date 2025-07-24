using System;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection for handling the change of a floorball match's venue after the event is stored.
    /// </summary>
    public sealed class FloorballMatchVenueChangedProjection : IDomainEventHandler<FloorballMatchVenueChangedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchVenueChangedProjection> _logger;

        public FloorballMatchVenueChangedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchVenueChangedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Updates the match's venue in the read model after the venue change event is stored
        /// </summary>
        public async Task HandleAsync(FloorballMatchVenueChangedEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballMatchVenueChangedEvent for match {MatchId}", domainEvent.MatchId);

            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Projection skipped – FloorballMatch {MatchId} not found", domainEvent.MatchId);
                return;
            }

            try
            {
                // Update the match's venue
                match.ChangeVenue(domainEvent.NewVenue ?? string.Empty);
                await _dbContext.SaveChangesWithoutEventsAsync();

                _logger.LogInformation("Projection updated FloorballMatch {MatchId} venue from '{PreviousVenue}' to '{NewVenue}'", 
                    domainEvent.MatchId, domainEvent.PreviousVenue, domainEvent.NewVenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when changing venue for FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
} 