using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.DomainEvents;
using MyLeague.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MyLeague.Infrastructure.DomainEvents.Handlers.Floorball.Projections
{
    /// <summary>
    /// Handles the <see cref="FloorballMatchCreatedEvent"/> to project a new
    /// <see cref="FloorballMatch"/> to the read model.
    /// </summary>
    public class ProjectFloorballMatchOnMatchCreated : IDomainEventHandler<FloorballMatchCreatedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<ProjectFloorballMatchOnMatchCreated> _logger;

        public ProjectFloorballMatchOnMatchCreated(
            FloorballDbContext dbContext,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<ProjectFloorballMatchOnMatchCreated> logger)
        {
            _dbContext = dbContext;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
        }

        public async Task HandleAsync(FloorballMatchCreatedEvent notification)
        {
            try
            {
                // Idempotency Check: Ensure we don't process the same event twice.
                bool alreadyExists = await _dbContext.FloorballMatches.AnyAsync(m => m.Id == notification.MatchId);
                if (alreadyExists)
                {
                    _logger.LogWarning("FloorballMatch read model for match ID {MatchId} already exists. Skipping projection.", notification.MatchId);
                    return;
                }

                // We create the read model without fetching related entities first.
                // This is more efficient as it avoids multiple database lookups.
                var match = new FloorballMatch(
                    notification.MatchId,
                    notification.SeasonId,
                    notification.HomeTeamId,
                    notification.AwayTeamId,
                    notification.ScheduledDateTime,
                    notification.Venue
                );

                await _dbContext.FloorballMatches.AddAsync(match);
                await _dbContext.SaveChangesAsync();
 
                _logger.LogInformation("Successfully projected new FloorballMatch read model for match ID {MatchId}", notification.MatchId);

                // Dispatch a new event to signal that the projection is complete
                var projectedEvent = new FloorballMatchProjectedEvent(notification.MatchId);
                await _domainEventDispatcher.DispatchAsync(projectedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error projecting FloorballMatch read model for match {MatchId}", notification.MatchId);
                throw;
            }
        }
    }
} 
