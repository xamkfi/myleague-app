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
            var eventsToSave = match.UncommittedEvents.ToList();
            if (!eventsToSave.Any())
            {
                _logger.LogInformation("No new events to save for match {MatchId}", match.Id);
                return;
            }

            // 1. Save all uncommitted events to the event store.
            // This also performs a concurrency check using the aggregate's current version.
            await _eventStore.SaveEventsAsync(
                match.Id,
                eventsToSave,
                match.Version, // The version of the aggregate before these new events
                cancellationToken);

            // 2. Mark events as committed on the aggregate. This updates its version number.
            match.MarkEventsAsCommitted();

            // 3. Update the read model/snapshot in the main database.
            EventSourcedFloorballMatch? snapshot = await _dbContext.EventSourcedFloorballMatches
                .FirstOrDefaultAsync(m => m.Id == match.Id, cancellationToken);

            if (snapshot == null)
            {
                // The match is new, add the entire aggregate as the snapshot.
                _dbContext.EventSourcedFloorballMatches.Add(match);
            }
            else
            {
                // The match exists, update its properties from the in-memory aggregate.
                // CurrentValues.SetValues copies scalar and complex type properties.
                // Navigation properties or collections are not updated, which is desired for a snapshot.
                _dbContext.Entry(snapshot).CurrentValues.SetValues(match);
            }

            // 4. Save the snapshot changes to the database.
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Saved {EventCount} new events for match {MatchId}. Snapshot is now at version {Version}",
                eventsToSave.Count,
                match.Id,
                match.Version);
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
