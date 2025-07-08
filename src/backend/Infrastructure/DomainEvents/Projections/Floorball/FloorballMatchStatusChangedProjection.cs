using System;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection for updating the status of a floorball match after a <see cref="FloorballMatchStatusChangedEvent"/>.
    /// </summary>
    public sealed class FloorballMatchStatusChangedProjection : IDomainEventHandler<FloorballMatchStatusChangedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchStatusChangedProjection> _logger;

        public FloorballMatchStatusChangedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchStatusChangedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task HandleAsync(FloorballMatchStatusChangedEvent domainEvent)
        {
            FloorballMatch? match = await _dbContext.FloorballMatches.FindAsync(domainEvent.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Projection skipped – FloorballMatch {MatchId} not found", domainEvent.MatchId);
                return;
            }
            
            try
            {
                switch (domainEvent.NewStatus)
                {
                    case FloorballMatchStatus.Postponed:
                        match.Postpone();
                        break;
                    case FloorballMatchStatus.InProgress:
                        match.Start();
                        break;
                    case FloorballMatchStatus.Completed:
                        match.Complete();
                        break;
                    case FloorballMatchStatus.Cancelled:
                        match.Cancel();
                        break;
                    case FloorballMatchStatus.Scheduled:
                        // No-op – reschedule projection handles updating scheduled matches.
                        break;
                    default:
                        _logger.LogDebug("No projection action required for status {Status}", domainEvent.NewStatus);
                        break;
                }

                await _dbContext.SaveChangesWithoutEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when updating status for FloorballMatch {MatchId}", domainEvent.MatchId);
            }
        }
    }
} 
