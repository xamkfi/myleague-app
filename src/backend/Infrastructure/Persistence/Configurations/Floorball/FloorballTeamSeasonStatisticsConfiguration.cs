using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTeamSeasonStatistics entity.
    /// </summary>
    public class FloorballTeamSeasonStatisticsConfiguration : IEntityTypeConfiguration<FloorballTeamSeasonStatistics>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballTeamSeasonStatistics.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballTeamSeasonStatistics> builder)
        {
            // Configure table
            builder.ToTable("FloorballTeamSeasonStatistics");
            
            // Configure primary key
            builder.HasKey(s => s.Id);

            // Configure foreign keys
            builder.Property(s => s.TeamId)
                .IsRequired()
                .HasComment("ID of the team these statistics belong to");

            builder.Property(s => s.CompetitionId)
                .IsRequired()
                .HasComment("ID of the competition these statistics are for");

            // Configure basic game statistics
            builder.Property(s => s.GamesPlayed)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of games played");

            builder.Property(s => s.Wins)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of wins");

            builder.Property(s => s.Losses)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of losses");

            builder.Property(s => s.Ties)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of ties/overtime losses");

            builder.Property(s => s.Points)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total points earned");

            // Configure scoring statistics
            builder.Property(s => s.GoalsFor)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total goals scored");

            builder.Property(s => s.GoalsAgainst)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total goals conceded");

            builder.Property(s => s.GoalDifference)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Goal difference (goals for - goals against)");

            // Configure shot statistics
            builder.Property(s => s.ShotsFor)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total shots taken");

            builder.Property(s => s.ShotsAgainst)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total shots faced");

            builder.Property(s => s.ShotPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Shot percentage");

            // Configure power play statistics
            builder.Property(s => s.PowerPlayGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play goals scored");

            builder.Property(s => s.PowerPlayOpportunities)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play opportunities");

            builder.Property(s => s.PowerPlayPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Power play success percentage");

            // Configure penalty kill statistics
            builder.Property(s => s.ShortHandedGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Short-handed goals scored");

            builder.Property(s => s.PenaltyKillOpportunities)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Penalty kill opportunities");

            builder.Property(s => s.PenaltyKillPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Penalty kill success percentage");

            // Configure penalty statistics
            builder.Property(s => s.PenaltyMinutes)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total penalty minutes");

            // Configure faceoff statistics
            builder.Property(s => s.FaceoffWins)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Faceoffs won");

            builder.Property(s => s.FaceoffAttempts)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total faceoffs taken");

            builder.Property(s => s.FaceoffPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Faceoff win percentage");

            // Configure home/away statistics
            builder.Property(s => s.HomeWins)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Home wins");

            builder.Property(s => s.HomeLosses)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Home losses");

            builder.Property(s => s.AwayWins)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Away wins");

            builder.Property(s => s.AwayLosses)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Away losses");

            // Configure indexes for performance
            builder.HasIndex(s => s.TeamId)
                .HasDatabaseName("IX_FloorballTeamSeasonStatistics_TeamId");

            builder.HasIndex(s => s.CompetitionId)
                .HasDatabaseName("IX_FloorballTeamSeasonStatistics_SeasonId");

            builder.HasIndex(s => new { s.TeamId, s.CompetitionId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballTeamSeasonStatistics_TeamId_SeasonId");

            // Configure base entity properties
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasComment("UTC timestamp when the entity was created");

            builder.Property(s => s.UpdatedAt)
                .HasComment("UTC timestamp when the entity was last updated");
        }
    }
}
