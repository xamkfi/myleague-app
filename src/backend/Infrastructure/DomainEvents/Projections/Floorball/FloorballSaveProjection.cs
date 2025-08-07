using System;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.DomainEvents;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    /// <summary>
    /// Projection to handle adding of save event to FloorballMatchEvents table.
    /// </summary>
    public class FloorballSaveProjection : IDomainEventHandler<FloorballSaveEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballSaveProjection> _logger;

        public FloorballSaveProjection(FloorballDbContext dbContext, ILogger<FloorballSaveProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Add the save after event storing.
        /// </summary>
        public async Task HandleAsync(FloorballSaveEvent domainEvent)
        {
            _logger.LogInformation("Handling FloorballSaveEvent for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);

            try
            {
                FloorballSave save = new FloorballSave(
                    Guid.NewGuid(),
                    domainEvent.MatchId,
                    domainEvent.TeamId,
                    domainEvent.GoalieId,
                    domainEvent.PeriodNumber,
                    domainEvent.TimeInSeconds,
                    domainEvent.WasInOvertime,
                    domainEvent.WasInShootout);

                // Correct: Add to FloorballMatchEvents for TPH mapping
                _dbContext.FloorballMatchEvents.Add(save);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully added save for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Projection failed when adding save for match {MatchId}, period {PeriodNumber}", domainEvent.MatchId, domainEvent.PeriodNumber);
            }
        }
    }
}
