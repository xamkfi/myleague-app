using Domain.DomainEvents;
using Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using Newtonsoft.Json;
using System.Text;

namespace MyLeague.Infrastructure.Persistence
{
    /// <summary>
    /// EventStore implementation for storing and retrieving domain events
    /// </summary>
    public class EventStore : IEventStore
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<EventStore> _logger;

        /// <summary>
        /// Initializes a new instance of the EventStore class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="logger">The logger</param>
        public EventStore(ApplicationDbContext dbContext, ILogger<EventStore> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default)
        {
            // Get current version from database
            int currentVersion = await GetAggregateVersionAsync(aggregateId, cancellationToken);

            // Check concurrency
            if (expectedVersion != -1 && currentVersion != expectedVersion)
            {
                _logger.LogWarning("Concurrency conflict when saving events for aggregate {AggregateId}. Expected version: {ExpectedVersion}, Actual version: {CurrentVersion}", 
                    aggregateId, expectedVersion, currentVersion);
                throw new DbUpdateConcurrencyException($"Concurrency conflict. Expected version {expectedVersion} but found {currentVersion}");
            }

            // Save each event
            foreach (IDomainEvent @event in events)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Saved {Count} events for aggregate {AggregateId}", events.Count(), aggregateId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            // Use the DomainEvents DbSet and filter by aggregate ID
            List<IDomainEvent> events = await _dbContext.Set<IDomainEvent>()
                .AsNoTracking()
                .Where(e => EF.Property<Guid>(e, "AggregateId") == aggregateId)
                .OrderBy(e => EF.Property<int>(e, "Version"))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} events for aggregate {AggregateId}", events.Count, aggregateId);
            return events;
        }

        /// <inheritdoc />
        public async Task<int> GetAggregateVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            // Query the maximum version for the aggregate
            int version = await _dbContext.Set<IDomainEvent>()
                .AsNoTracking()
                .Where(e => EF.Property<Guid>(e, "AggregateId") == aggregateId)
                .Select(e => EF.Property<int>(e, "Version"))
                .DefaultIfEmpty(-1)
                .MaxAsync(cancellationToken);

            return version;
        }
    }
} 