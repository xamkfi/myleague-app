using Microsoft.EntityFrameworkCore;
using Domain.Entities.Common;
using Domain.DomainEvents;
using MyLeague.Infrastructure.DomainEvents;
using MyLeague.Infrastructure.Persistence.Extensions;
using MyLeague.Infrastructure.Persistence.Configurations.Common;
using System.Reflection;

namespace MyLeague.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// Database context for common entities in the MyLeague application.
    /// </summary>
    public class CommonDbContext : DbContext
    {
        private readonly IDomainEventDispatcher? _dispatcher;
        private bool _isDispatchingEvents = false;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        /// <param name="dispatcher">The domain event dispatcher to use.</param>
        public CommonDbContext(
            DbContextOptions<CommonDbContext> options,
            IDomainEventDispatcher? dispatcher = null)
            : base(options)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Gets or sets the Persons DbSet.
        /// </summary>
        public DbSet<Person> Persons { get; set; }

        /// <summary>
        /// Gets or sets the Clubs DbSet.
        /// </summary>
        public DbSet<Club> Clubs { get; set; }
        
        /// <summary>
        /// Saves changes to the database with domain event dispatching.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Prevent infinite recursion when called from SaveChangesWithEventsAsync
            if (_isDispatchingEvents || _dispatcher == null)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            return await this.SaveChangesWithEventsAsync(_dispatcher, cancellationToken);
        }

        /// <summary>
        /// Saves changes without dispatching domain events (used internally to prevent recursion).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of state entries written to the database</returns>
        internal async Task<int> SaveChangesWithoutEventsAsync(CancellationToken cancellationToken = default)
        {
            _isDispatchingEvents = true;
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _isDispatchingEvents = false;
            }
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply only Common configurations to avoid cross-context conflicts
            modelBuilder.ApplyConfiguration(new PersonConfiguration());
            modelBuilder.ApplyConfiguration(new ClubConfiguration());
        }
    }
} 