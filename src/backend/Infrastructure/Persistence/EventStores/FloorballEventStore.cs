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

namespace MyLeague.Infrastructure.Persistence.EventStores
{
    /// <summary>
    /// Floorball-specific EventStore implementation for storing and retrieving domain events
    /// </summary>
    public class FloorballEventStore : IEventStore
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
                EntityEntry<IDomainEvent> entry = _floorballDbContext.Entry(@event);
                entry.Property("AggregateId").CurrentValue = aggregateId;
                entry.Property("Version").CurrentValue = version;
                
                // Add the event to the context
                _floorballDbContext.Add(@event);
            }

            // Save all events in a single transaction
            await _floorballDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved {Count} events for aggregate {AggregateId}", events.Count(), aggregateId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
        {
            try
            {
                List<IDomainEvent> events = await _floorballDbContext.Set<IDomainEvent>()
                    .AsNoTracking()
                    .Where(e => EF.Property<Guid>(e, "AggregateId") == aggregateId)
                    .OrderBy(e => EF.Property<int>(e, "Version"))
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} events for aggregate {AggregateId}", events.Count, aggregateId);
                return events;
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
                return await _floorballDbContext.Set<IDomainEvent>()
                    .AsNoTracking()
                    .Where(e => EF.Property<Guid>(e, "AggregateId") == aggregateId)
                    .Select(e => EF.Property<int>(e, "Version"))
                    .DefaultIfEmpty(-1)
                    .MaxAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregate version for {AggregateId}", aggregateId);
                return -1;
            }
        }
    }
} 