using Domain.Entities.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballMatch entity.
    /// </summary>
    public class FloorballMatchConfiguration : IEntityTypeConfiguration<FloorballMatch>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballMatch.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballMatch> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.ScheduledDateTime)
                .IsRequired();

            builder.Property(m => m.Venue)
                .HasMaxLength(200);

            builder.Property(m => m.HomeScore)
                .IsRequired();

            builder.Property(m => m.AwayScore)
                .IsRequired();

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.WentToOvertime)
                .IsRequired();

            builder.Property(m => m.WentToShootout)
                .IsRequired();

            // Configure foreign key relationships with navigation properties within the same context
            builder.Property(m => m.HomeTeamId)
                .IsRequired();

            builder.Property(m => m.AwayTeamId)
                .IsRequired();

            builder.Property(m => m.SeasonId)
                .IsRequired();

            // Configure relationships within FloorballDbContext
            builder.HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship with referees using a simple many-to-many join table
            builder.HasMany(m => m.Officials)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "FloorballMatchOfficial",
                    j => j.HasOne<FloorballReferee>().WithMany().HasForeignKey("OfficialsId"),
                    j => j.HasOne<FloorballMatch>().WithMany().HasForeignKey("MatchesId")
                );

            // Configure owned types for period scores
            builder.OwnsMany(m => m.PeriodScores, periodBuilder =>
            {
                periodBuilder.WithOwner().HasForeignKey("MatchId");
                periodBuilder.Property<int>("Id").ValueGeneratedOnAdd();
                periodBuilder.HasKey("Id");

                periodBuilder.Property(p => p.PeriodNumber)
                    .IsRequired();

                periodBuilder.Property(p => p.HomeScore)
                    .IsRequired();

                periodBuilder.Property(p => p.AwayScore)
                    .IsRequired();
            });

            // Ignore complex event configurations for now to avoid navigationName issues
            builder.Ignore(m => m.Events);
            builder.Ignore(m => m.GoalEvents);
            builder.Ignore(m => m.PenaltyEvents);
        }
    }
} 