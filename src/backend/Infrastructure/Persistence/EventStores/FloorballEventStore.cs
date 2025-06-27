using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Domain.DomainEvents.Floorball;
using Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Persistence.EventStores
{
    /// <summary>
    /// Floorball-specific EventStore implementation for storing and retrieving domain events
    /// </summary>
    public class FloorballEventStore : IFloorballEventStore
    {
        private readonly FloorballDbContext _floorballDbContext;
        private readonly ILogger<FloorballEventStore> _logger;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Initializes a new instance of the FloorballEventStore class
        /// </summary>
        /// <param name="floorballDbContext">The floorball database context</param>
        /// <param name="logger">The logger</param>
        public FloorballEventStore(
            FloorballDbContext floorballDbContext,
            ILogger<FloorballEventStore> logger)
        {
            _floorballDbContext = floorballDbContext;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default)
        {
            int currentVersion = await GetAggregateVersionAsync(aggregateId, cancellationToken);
            if (currentVersion != expectedVersion)
            {
                _logger.LogWarning("Concurrency conflict for aggregate {AggregateId}. Expected: {ExpectedVersion}, Actual: {CurrentVersion}", aggregateId, expectedVersion, currentVersion);
                throw new DbUpdateConcurrencyException($"Concurrency conflict. Expected version {expectedVersion} but found {currentVersion}");
            }

            var eventEntities = new List<FloorballDomainEvent>();
            int version = currentVersion;

            foreach (FloorballDomainEvent domainEvent in events.Cast<FloorballDomainEvent>())
            {
                version++;
                domainEvent.Data = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonSerializerOptions);
                
                EntityEntry<FloorballDomainEvent> entry = _floorballDbContext.Entry(domainEvent);
                entry.Property("AggregateId").CurrentValue = aggregateId;
                entry.Property("Version").CurrentValue = version;

                eventEntities.Add(domainEvent);
            }

            _floorballDbContext.Set<FloorballDomainEvent>().AddRange(eventEntities);
            await _floorballDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved {Count} events for aggregate {AggregateId}", eventEntities.Count, aggregateId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            List<FloorballDomainEvent> eventEntities = await _floorballDbContext.Set<FloorballDomainEvent>()
                .AsNoTracking()
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => EF.Property<int>(e, "Version"))
                .ToListAsync(cancellationToken);

            if (!eventEntities.Any())
            {
                return Enumerable.Empty<IDomainEvent>();
            }

            var domainEvents = new List<IDomainEvent>();
            foreach (FloorballDomainEvent entity in eventEntities)
            {
                var eventType = Type.GetType($"Domain.DomainEvents.Floorball.{entity.EventType}, Domain");
                if (eventType == null)
                {
                    _logger.LogWarning("Could not find event type {EventType}", entity.EventType);
                    continue;
                }

                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(entity.Data, eventType, _jsonSerializerOptions)!;
                domainEvents.Add(domainEvent);
            }

            _logger.LogInformation("Retrieved {Count} events for aggregate {AggregateId}", domainEvents.Count, aggregateId);
            return domainEvents;
        }

        /// <inheritdoc />
        public async Task<int> GetAggregateVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            try
            {
                int? maxVersion = await _floorballDbContext.Set<FloorballDomainEvent>()
                    .AsNoTracking()
                    .Where(e => e.AggregateId == aggregateId)
                    .Select(e => (int?)EF.Property<int>(e, "Version"))
                    .MaxAsync(cancellationToken);

                return maxVersion ?? -1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregate version for {AggregateId}", aggregateId);
                return -1;
            }
        }
    }
} 
