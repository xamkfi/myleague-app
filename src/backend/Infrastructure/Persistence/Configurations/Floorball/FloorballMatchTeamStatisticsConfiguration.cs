using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballMatchTeamStatistics entity.
    /// </summary>
    public class FloorballMatchTeamStatisticsConfiguration : IEntityTypeConfiguration<FloorballMatchTeamStatistics>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballMatchTeamStatistics.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballMatchTeamStatistics> builder)
        {
            // Configure table
            builder.ToTable("FloorballMatchTeamStatistics");
            
            // Configure primary key
            builder.HasKey(s => s.Id);

            // Configure foreign keys
            builder.Property(s => s.MatchId)
                .IsRequired()
                .HasComment("ID of the match these statistics belong to");

            builder.Property(s => s.TeamId)
                .IsRequired()
                .HasComment("ID of the team these statistics are for");

            // Ignore navigation properties to prevent cross-context issues
            builder.Ignore(s => s.Match);
            builder.Ignore(s => s.Team);

            // Configure shot statistics
            builder.Property(s => s.ShotsOnGoal)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Shots on goal");

            builder.Property(s => s.ShotsTotal)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total shots taken");

            builder.Property(s => s.ShotPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Shot percentage");

            // Configure faceoff statistics
            builder.Property(s => s.FaceoffWins)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Faceoffs won");

            builder.Property(s => s.FaceoffAttempts)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total faceoffs");

            builder.Property(s => s.FaceoffPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Faceoff win percentage");

            // Configure power play statistics
            builder.Property(s => s.PowerPlayOpportunities)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play opportunities");

            builder.Property(s => s.PowerPlayGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play goals");

            builder.Property(s => s.PowerPlayMinutes)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play minutes");

            // Configure penalty kill statistics
            builder.Property(s => s.PenaltyKillOpportunities)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Penalty kill opportunities");

            builder.Property(s => s.PenaltyKillSuccess)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Successful penalty kills");

            builder.Property(s => s.ShortHandedGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Short-handed goals");

            // Configure penalty statistics
            builder.Property(s => s.PenaltyMinutes)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Penalty minutes");

            // Configure physical play statistics
            builder.Property(s => s.Hits)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Hits delivered");

            builder.Property(s => s.BlockedShots)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Shots blocked");

            builder.Property(s => s.Takeaways)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Takeaways");

            builder.Property(s => s.Giveaways)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Giveaways");

            // Configure indexes for performance
            builder.HasIndex(s => s.MatchId)
                .HasDatabaseName("IX_FloorballMatchTeamStatistics_MatchId");

            builder.HasIndex(s => s.TeamId)
                .HasDatabaseName("IX_FloorballMatchTeamStatistics_TeamId");

            builder.HasIndex(s => new { s.MatchId, s.TeamId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballMatchTeamStatistics_MatchId_TeamId");

            // Configure base entity properties
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasComment("UTC timestamp when the entity was created");

            builder.Property(s => s.UpdatedAt)
                .HasComment("UTC timestamp when the entity was last updated");
        }
    }
}
