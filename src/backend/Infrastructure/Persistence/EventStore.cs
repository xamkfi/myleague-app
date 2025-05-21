using Domain.DomainEvents;
using Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using Newtonsoft.Json;
using System.Text;
using Domain.DomainEvents.Common;
using Domain.DomainEvents.Floorball;

namespace MyLeague.Infrastructure.Persistence
{
    /// <summary>
    /// EventStore implementation for storing and retrieving domain events
    /// </summary>
    public class EventStore : IEventStore
    {
        private readonly FloorballDbContext _floorballDbContext;
        private readonly CommonDbContext _commonDbContext;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<EventStore> _logger;

        /// <summary>
        /// Initializes a new instance of the EventStore class
        /// </summary>
        /// <param name="floorballDbContext">The floorball database context</param>
        /// <param name="commonDbContext">The common database context</param>
        /// <param name="applicationDbContext">The application database context</param>
        /// <param name="logger">The logger</param>
        public EventStore(
            FloorballDbContext floorballDbContext, 
            CommonDbContext commonDbContext,
            ApplicationDbContext applicationDbContext,
            ILogger<EventStore> logger)
        {
            _floorballDbContext = floorballDbContext;
            _commonDbContext = commonDbContext;
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default)
        {
            // Get the appropriate context based on the first event type
            DbContext context = GetContextForEvents(events);

            // Get current version from database
            int currentVersion = await GetAggregateVersionAsync(aggregateId, cancellationToken);

            // Check concurrency
            if (expectedVersion != -1 && currentVersion != expectedVersion)
            {
                _logger.LogWarning("Concurrency conflict when saving events for aggregate {AggregateId}. Expected version: {ExpectedVersion}, Actual version: {CurrentVersion}", 
                    aggregateId, expectedVersion, currentVersion);
                throw new DbUpdateConcurrencyException($"Concurrency conflict. Expected version {expectedVersion} but found {currentVersion}");
            }

            int version = currentVersion;

            // Add each event to DbContext
            foreach (IDomainEvent @event in events)
            {
                version++;
                
                // Set the aggregate ID and version properties
                EntityEntry<IDomainEvent> entry = context.Entry(@event);
                entry.Property("AggregateId").CurrentValue = aggregateId;
                entry.Property("Version").CurrentValue = version;
                
                // Add the event to the context
                context.Add(@event);
            }

            // Save all events in a single transaction
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved {Count} events for aggregate {AggregateId}", events.Count(), aggregateId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            // Try to get events from all contexts and combine them
            IEnumerable<IDomainEvent> floorballEvents = await GetEventsFromContext(_floorballDbContext, aggregateId, cancellationToken);
            IEnumerable<IDomainEvent> commonEvents = await GetEventsFromContext(_commonDbContext, aggregateId, cancellationToken);
            IEnumerable<IDomainEvent> appEvents = await GetEventsFromContext(_applicationDbContext, aggregateId, cancellationToken);

            var allEvents = floorballEvents.Concat(commonEvents).Concat(appEvents)
                .OrderBy(e => EF.Property<int>(e, "Version"))
                .ToList();

            _logger.LogInformation("Retrieved {Count} events for aggregate {AggregateId}", allEvents.Count, aggregateId);
            return allEvents;
        }

        private async Task<IEnumerable<IDomainEvent>> GetEventsFromContext(DbContext context, Guid aggregateId, CancellationToken cancellationToken)
        {
            try
            {
                return await context.Set<IDomainEvent>()
                    .AsNoTracking()
                    .Where(e => EF.Property<Guid>(e, "AggregateId") == aggregateId)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events from context {ContextName}", context.GetType().Name);
                return Enumerable.Empty<IDomainEvent>();
            }
        }

        /// <inheritdoc />
        public async Task<int> GetAggregateVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            // Get max version from each context
            int floorballVersion = await GetAggregateVersionFromContext(_floorballDbContext, aggregateId, cancellationToken);
            int commonVersion = await GetAggregateVersionFromContext(_commonDbContext, aggregateId, cancellationToken);
            int appVersion = await GetAggregateVersionFromContext(_applicationDbContext, aggregateId, cancellationToken);

            // Return the highest version
            return Math.Max(Math.Max(floorballVersion, commonVersion), appVersion);
        }

        private async Task<int> GetAggregateVersionFromContext(DbContext context, Guid aggregateId, CancellationToken cancellationToken)
        {
            try
            {
                return await context.Set<IDomainEvent>()
                    .AsNoTracking()
                    .Where(e => EF.Property<Guid>(e, "AggregateId") == aggregateId)
                    .Select(e => EF.Property<int>(e, "Version"))
                    .DefaultIfEmpty(-1)
                    .MaxAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregate version from context {ContextName}", context.GetType().Name);
                return -1;
            }
        }

        private DbContext GetContextForEvents(IEnumerable<IDomainEvent> events)
        {
            if (!events.Any())
                return _applicationDbContext;

            IDomainEvent firstEvent = events.First();
            
            if (firstEvent is FloorballDomainEvent)
                return _floorballDbContext;
            
            if (firstEvent is CommonDomainEvent)
                return _commonDbContext;
            
            return _applicationDbContext;
        }
    }
} 
