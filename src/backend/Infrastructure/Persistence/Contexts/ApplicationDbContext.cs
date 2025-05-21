using Microsoft.EntityFrameworkCore;
using Domain.Entities.Common;
using Domain.Entities.Floorball;

namespace MyLeague.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// Main database context for the MyLeague application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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
        /// Configures the model that was discovered by convention from the entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from the Configurations namespace
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
} 
