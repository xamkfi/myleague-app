using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballPeriodScore entity.
    /// </summary>
    public class FloorballPeriodScoreConfiguration : BaseEntityConfiguration<FloorballPeriodScore>
    {
        /// <summary>
        /// Configures the entity-specific properties for FloorballPeriodScore.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        protected override void ConfigureEntity(EntityTypeBuilder<FloorballPeriodScore> builder)
        {
            // Configure table name
            builder.ToTable("FloorballPeriodScores");

            // Configure required properties
            builder.Property(p => p.MatchId)
                .IsRequired()
                .HasComment("ID of the match this period score belongs to");

            builder.Property(p => p.PeriodNumber)
                .IsRequired()
                .HasComment("The period number (1, 2, 3, etc.)");

            builder.Property(p => p.HomeTeamId)
                .IsRequired()
                .HasComment("ID of the home team");

            builder.Property(p => p.AwayTeamId)
                .IsRequired()
                .HasComment("ID of the away team");

            builder.Property(p => p.HomeScore)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Home team score for this period");

            builder.Property(p => p.AwayScore)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Away team score for this period");

            builder.Property(p => p.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Whether the period is completed");

            // Configure foreign key relationship with FloorballMatch
            builder.HasOne<FloorballMatch>()
                .WithMany(m => m.PeriodScores)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            builder.HasIndex(p => new { p.MatchId, p.PeriodNumber })
                .IsUnique()
                .HasDatabaseName("IX_FloorballPeriodScore_Match_Period");

            builder.HasIndex(p => p.MatchId)
                .HasDatabaseName("IX_FloorballPeriodScore_MatchId");
        }
    }
} 