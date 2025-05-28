using Domain.DomainEvents;
using Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.DomainEvents;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Reflection;

namespace MyLeague.Infrastructure.Persistence.Extensions
{
    /// <summary>
    /// Extension methods for DbContext to handle domain events
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Dispatches domain events before saving changes
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="dispatcher">The domain event dispatcher</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of state entries written to the database</returns>
        public static async Task<int> SaveChangesWithEventsAsync(
            this DbContext dbContext,
            IDomainEventDispatcher dispatcher,
            CancellationToken cancellationToken = default)
        {
            // Get all the entities that implement IAggregateRoot
            var aggregateRoots = dbContext.ChangeTracker.Entries<AggregateRoot>()
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            // Collect all domain events from aggregate roots
            var domainEvents = aggregateRoots
                .SelectMany(x => x.DomainEvents)
                .ToList();

            // Clear the domain events from the aggregate roots
            aggregateRoots.ForEach(aggregate => aggregate.ClearDomainEvents());

            // Save changes to the database using the appropriate method to avoid recursion
            int result;
            
            if (dbContext is CommonDbContext commonDbContext)
            {
                // Use the internal method that bypasses event dispatching to prevent recursion
                result = await commonDbContext.SaveChangesWithoutEventsAsync(cancellationToken);
            }
            else if (dbContext is FloorballDbContext floorballDbContext)
            {
                // Use the internal method that bypasses event dispatching to prevent recursion
                result = await floorballDbContext.SaveChangesWithoutEventsAsync(cancellationToken);
            }
            else
            {
                // For other DbContext types, call the base SaveChangesAsync directly
                result = await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Dispatch the domain events after the changes have been saved
            await dispatcher.DispatchAsync(domainEvents);

            return result;
        }
    }
} 