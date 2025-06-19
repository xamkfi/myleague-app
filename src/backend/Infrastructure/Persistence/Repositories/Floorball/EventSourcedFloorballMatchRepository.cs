using Domain.DomainEvents;
using Domain.Entities.Floorball;
using Domain.EventSourcing;
using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.Repositories.Floorball
{
    /// <summary>
    /// Repository implementation for event-sourced floorball matches
    /// </summary>
    public class EventSourcedFloorballMatchRepository : IEventSourcedFloorballMatchRepository
    {
        private readonly FloorballDbContext _dbContext;
        private readonly IFloorballEventStore _eventStore;
        private readonly ILogger<EventSourcedFloorballMatchRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the EventSourcedFloorballMatchRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="eventStore">The event store</param>
        /// <param name="logger">The logger</param>
        public EventSourcedFloorballMatchRepository(
            FloorballDbContext dbContext,
            IFloorballEventStore eventStore,
            ILogger<EventSourcedFloorballMatchRepository> logger)
        {
            _dbContext = dbContext;
            _eventStore = eventStore;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<EventSourcedFloorballMatch> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Check if the match exists
            bool exists = await _dbContext.EventSourcedFloorballMatches
                .AsNoTracking()
                .AnyAsync(m => m.Id == id, cancellationToken);

            if (!exists)
            {
                _logger.LogWarning("Event sourced floorball match with ID {MatchId} not found", id);
                throw new KeyNotFoundException($"Event sourced floorball match with ID {id} not found");
            }

            // Create new instance that will be hydrated from events
            EventSourcedFloorballMatch match = new EventSourcedFloorballMatch();
            
            // Get all events for this match
            IEnumerable<IDomainEvent> eventHistory = await _eventStore.GetEventsAsync(id, cancellationToken);
            
            // Apply events to rebuild the match state
            match.LoadFromHistory(eventHistory);
            
            _logger.LogInformation("Retrieved event sourced floorball match {MatchId} with {EventCount} events", id, eventHistory.Count());
            
            return match;
        }

        /// <inheritdoc />
        public async Task SaveAsync(EventSourcedFloorballMatch match, CancellationToken cancellationToken = default)
        {
            // If the match is not in the database yet, add it
            if (!await _dbContext.EventSourcedFloorballMatches.AnyAsync(m => m.Id == match.Id, cancellationToken))
            {
                _dbContext.EventSourcedFloorballMatches.Add(match);
            }
            
            // Save all uncommitted events to the event store
            await _eventStore.SaveEventsAsync(
                match.Id,
                match.UncommittedEvents,
                match.Version,
                cancellationToken);
            
            // Mark events as committed so they won't be saved again
            match.MarkEventsAsCommitted();
            
            _logger.LogInformation("Saved event sourced floorball match {MatchId} with {EventCount} new events", 
                match.Id, match.UncommittedEvents.Count);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDomainEvent>> GetHistoryAsync(Guid matchId, CancellationToken cancellationToken = default)
        {
            // Get all events for this match from the event store
            IEnumerable<IDomainEvent> events = await _eventStore.GetEventsAsync(matchId, cancellationToken);
            
            _logger.LogInformation("Retrieved {EventCount} historical events for match {MatchId}", events.Count(), matchId);
            
            return events;
        }
    }
} 