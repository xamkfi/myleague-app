using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using Domain.DomainEvents.Common;
using Domain.DomainEvents.Floorball;
using Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyLeague.Infrastructure.Persistence.EventStores
{
    /// <summary>
    /// Floorball-specific EventStore implementation for storing and retrieving domain events
    /// </summary>
    public class FloorballEventStore : IFloorballEventStore
    {
        private readonly FloorballDbContext _floorballDbContext;
        private readonly ILogger<FloorballEventStore> _logger;

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
            // Get current version from the event store (–1 when no events yet)
            int currentVersion = await GetAggregateVersionAsync(aggregateId, cancellationToken);

            // Check optimistic concurrency
            if (expectedVersion != -1 && currentVersion != expectedVersion)
            {
                _logger.LogWarning("Concurrency conflict when saving events for aggregate {AggregateId}. Expected version: {ExpectedVersion}, Actual version: {CurrentVersion}",
                    aggregateId, expectedVersion, currentVersion);
                throw new DbUpdateConcurrencyException($"Concurrency conflict. Expected version {expectedVersion} but found {currentVersion}");
            }

            int version = currentVersion;

            // Common serializer options (enum as strings, case-insensitive reading)
            JsonSerializerOptions serializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                Converters = { new JsonStringEnumConverter() }
            };

            foreach (IDomainEvent @event in events)
            {
                version++;

                var storedEvent = new FloorballStoredEvent
                {
                    Id = Guid.NewGuid(),
                    AggregateId = aggregateId,
                    EventType = @event.GetType().AssemblyQualifiedName!,
                    Data = JsonSerializer.Serialize(@event, @event.GetType(), serializerOptions),
                    Version = version,
                    OccurredOn = @event.OccurredOn
                };

                _floorballDbContext.FloorballStoredEvents.Add(storedEvent);
            }

            // Persist all events in a single transaction
            await _floorballDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved {Count} events for aggregate {AggregateId}", events.Count(), aggregateId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<FloorballStoredEvent> storedEvents = await _floorballDbContext.FloorballStoredEvents
                    .AsNoTracking()
                    .Where(e => e.AggregateId == aggregateId)
                    .OrderBy(e => e.Version)
                    .ToListAsync(cancellationToken);

                var domainEvents = new List<IDomainEvent>();
                JsonSerializerOptions serializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                foreach (FloorballStoredEvent storedEvent in storedEvents)
                {
                    Type? eventType = Type.GetType(storedEvent.EventType);
                    if (eventType == null)
                    {
                        _logger.LogWarning("Event type {EventType} could not be resolved when loading aggregate {AggregateId}", storedEvent.EventType, aggregateId);
                        continue;
                    }

                    try
                    {
                        IDomainEvent? domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(storedEvent.Data, eventType, serializerOptions);
                        if (domainEvent != null)
                        {
                            domainEvents.Add(domainEvent);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to deserialize event {EventType} for aggregate {AggregateId}", storedEvent.EventType, aggregateId);
                    }
                }

                _logger.LogInformation("Retrieved {Count} events for aggregate {AggregateId}", domainEvents.Count, aggregateId);
                return domainEvents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events for aggregate {AggregateId}", aggregateId);
                return Enumerable.Empty<IDomainEvent>();
            }
        }

        /// <inheritdoc />
        public async Task<int> GetAggregateVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            try
            {
                int? maxVersion = await _floorballDbContext.FloorballStoredEvents
                    .AsNoTracking()
                    .Where(e => e.AggregateId == aggregateId)
                    .Select(e => (int?)e.Version)
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
