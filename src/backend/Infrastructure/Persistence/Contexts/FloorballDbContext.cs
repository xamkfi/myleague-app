using Microsoft.EntityFrameworkCore;
using Domain.Entities.Floorball;
using Domain.DomainEvents;
using MyLeague.Infrastructure.DomainEvents;
using MyLeague.Infrastructure.Persistence.Extensions;
using MyLeague.Infrastructure.Persistence.Configurations.Floorball;
using System.Reflection;

namespace MyLeague.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// Database context for floorball-specific entities in the MyLeague application.
    /// </summary>
    public class FloorballDbContext : DbContext
    {
        private readonly IDomainEventDispatcher? _dispatcher;
        private bool _isDispatchingEvents = false;
        
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
        /// Gets or sets the FloorballPeriodScores DbSet.
        /// </summary>
        public DbSet<FloorballPeriodScore> FloorballPeriodScores { get; set; }

        /// <summary>
        /// Gets or sets the FloorballMatchEvents DbSet.
        /// </summary>
        public DbSet<FloorballMatchEvent> FloorballMatchEvents { get; set; }

        /// <summary>
        /// Gets or sets the FloorballGoals DbSet.
        /// </summary>
        public DbSet<FloorballGoal> FloorballGoals { get; set; }

        /// <summary>
        /// Gets or sets the FloorballPenalties DbSet.
        /// </summary>
        public DbSet<FloorballPenalty> FloorballPenalties { get; set; }

        /// <summary>
        /// Gets or sets the FloorballTeamManagers DbSet.
        /// </summary>
        public DbSet<FloorballTeamManager> FloorballTeamManagers { get; set; }

        /// <summary>
        /// Gets or sets the FloorballCoaches DbSet.
        /// </summary>
        public DbSet<FloorballCoach> FloorballCoaches { get; set; }
        
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

            // Set default schema for all Floorball entities
            modelBuilder.HasDefaultSchema("floorball");

            // Apply only Floorball configurations to avoid cross-context conflicts
            modelBuilder.ApplyConfiguration(new FloorballPlayerConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballTeamConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballMatchConfiguration());
            modelBuilder.ApplyConfiguration(new EventSourcedFloorballMatchConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballSeasonConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballRefereeConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballPeriodScoreConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballMatchEventConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballTeamManagerConfiguration());
            modelBuilder.ApplyConfiguration(new FloorballCoachConfiguration());
        }
    }
} 