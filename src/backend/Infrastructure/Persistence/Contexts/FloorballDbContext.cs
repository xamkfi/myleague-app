using Microsoft.EntityFrameworkCore;
using Domain.Entities.Floorball;
using Domain.DomainEvents;
using MyLeague.Infrastructure.DomainEvents;
using MyLeague.Infrastructure.Persistence.Extensions;
using System.Reflection;

namespace MyLeague.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// Database context for floorball-specific entities in the MyLeague application.
    /// </summary>
    public class FloorballDbContext : DbContext
    {
        private readonly IDomainEventDispatcher? _dispatcher;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FloorballDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        /// <param name="dispatcher">The domain event dispatcher to use.</param>
        public FloorballDbContext(
            DbContextOptions<FloorballDbContext> options,
            IDomainEventDispatcher? dispatcher = null)
            : base(options)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// Gets or sets the FloorballPlayers DbSet.
        /// </summary>
        public DbSet<FloorballPlayer> FloorballPlayers { get; set; }

        /// <summary>
        /// Gets or sets the FloorballTeams DbSet.
        /// </summary>
        public DbSet<FloorballTeam> FloorballTeams { get; set; }

        /// <summary>
        /// Gets or sets the FloorballMatches DbSet.
        /// </summary>
        public DbSet<FloorballMatch> FloorballMatches { get; set; }

        /// <summary>
        /// Gets or sets the EventSourcedFloorballMatches DbSet.
        /// </summary>
        public DbSet<EventSourcedFloorballMatch> EventSourcedFloorballMatches { get; set; }

        /// <summary>
        /// Gets or sets the FloorballSeasons DbSet.
        /// </summary>
        public DbSet<FloorballSeason> FloorballSeasons { get; set; }

        /// <summary>
        /// Gets or sets the FloorballReferees DbSet.
        /// </summary>
        public DbSet<FloorballReferee> FloorballReferees { get; set; }
        
        /// <summary>
        /// Saves changes to the database with domain event dispatching.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_dispatcher != null)
            {
                return await this.SaveChangesWithEventsAsync(_dispatcher);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from the Configurations namespace
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
} 