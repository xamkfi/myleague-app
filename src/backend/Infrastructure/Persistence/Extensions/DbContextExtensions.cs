using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Reflection;

namespace MyLeague.Infrastructure.Persistence.Extensions
{
    /// <summary>
    /// Extension methods for DbContext to handle domain events and audit fields
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Dispatches domain events and updates audit fields before saving changes
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="dispatcher">The domain event dispatcher</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of state entries written to the database</returns>
        public static async Task<int> SaveChangesWithEventsAsync(
            this DbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            // Update audit fields before processing domain events
            UpdateAuditFields(dbContext);

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

            return result;
        }

        /// <summary>
        /// Updates audit fields for all tracked entities that inherit from BaseEntity
        /// </summary>
        /// <param name="dbContext">The database context</param>
        private static void UpdateAuditFields(DbContext dbContext)
        {
            var auditableEntries = dbContext.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            DateTime now = DateTime.UtcNow;

            foreach (EntityEntry<BaseEntity> entry in auditableEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // For new entities, CreatedAt is already set in the constructor
                        // Ensure it's set to a consistent timestamp if needed
                        if (entry.Entity.CreatedAt == default)
                        {
                            // Use reflection to set CreatedAt if it wasn't set in constructor
                            PropertyEntry? createdAtProperty = entry.Property(nameof(BaseEntity.CreatedAt));
                            createdAtProperty.CurrentValue = now;
                        }
                        break;

                    case EntityState.Modified:
                        // For modified entities, update the UpdatedAt timestamp
                        // Prevent updating CreatedAt on modifications
                        entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                        
                        // Update the UpdatedAt timestamp using the internal method
                        entry.Entity.SetUpdatedAt(now);
                        break;
                }
            }
        }
    }
} 
