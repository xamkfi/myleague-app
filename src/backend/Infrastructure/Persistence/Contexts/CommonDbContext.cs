using Microsoft.EntityFrameworkCore;
using Domain.Entities.Common;
using MyLeague.Infrastructure.Persistence.Extensions;
using MyLeague.Infrastructure.Persistence.Configurations.Common;
using Application.Interfaces.Common;

namespace MyLeague.Infrastructure.Persistence.Contexts
{
    /// <summary>
    /// Database context for common entities in the MyLeague application.
    /// </summary>
    public class CommonDbContext : DbContext, ICommonDbContext
    {
        private bool _isDispatchingEvents = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public CommonDbContext(DbContextOptions<CommonDbContext> options) : base(options) {}

        /// <summary>
        /// Gets or sets the Persons DbSet.
        /// </summary>
        public DbSet<Person> Persons { get; set; }

        /// <summary>
        /// Gets or sets the Users DbSet.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the Clubs DbSet.
        /// </summary>
        public DbSet<Club> Clubs { get; set; }

        /// <summary>
        /// Gets or sets the NewsArticles DbSet.
        /// </summary>
        public DbSet<NewsArticle> NewsArticles { get; set; }

        /// <summary>
        /// Gets or sets the InfoPageContents DbSet.
        /// </summary>
        public DbSet<InfoPageContent> InfoPageContents { get; set; }

        /// <summary>
        /// Gets or sets the RulesSections DbSet.
        /// </summary>
        public DbSet<RulesSection> RulesSections { get; set; }

        /// <summary>
        /// Gets or sets the Divisions DbSet.
        /// </summary>
        public DbSet<Division> Divisions { get; set; }

        /// <summary>
        /// Gets or sets the RefreshTokens DbSet.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Gets or sets the TimerStates DbSet.
        /// </summary>
        public DbSet<TimerState> TimerStates { get; set; }

        /// <summary>
        /// Saves changes to the database with domain event dispatching.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Prevent infinite recursion when called from SaveChangesWithEventsAsync
            if (_isDispatchingEvents)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            return await this.SaveChangesWithEventsAsync(cancellationToken);
        }

        /// <summary>
        /// Saves changes without dispatching domain events.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
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

            // Set default schema for all Common entities
            modelBuilder.HasDefaultSchema("common");

            // Apply only Common configurations to avoid cross-context conflicts
            modelBuilder.ApplyConfiguration(new PersonConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ClubConfiguration());
            modelBuilder.ApplyConfiguration(new NewsArticleConfiguration());
            modelBuilder.ApplyConfiguration(new InfoPageContentConfiguration());
            modelBuilder.ApplyConfiguration(new RulesSectionConfiguration());
            modelBuilder.ApplyConfiguration(new DivisionConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new TimerStateConfiguration());
        }
    }
}