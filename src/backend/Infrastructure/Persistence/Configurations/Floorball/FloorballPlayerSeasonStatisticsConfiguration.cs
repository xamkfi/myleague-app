using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballPlayerSeasonStatistics entity.
    /// </summary>
    public class FloorballPlayerSeasonStatisticsConfiguration : IEntityTypeConfiguration<FloorballPlayerSeasonStatistics>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballPlayerSeasonStatistics.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballPlayerSeasonStatistics> builder)
        {
            // Configure table
            builder.ToTable("FloorballPlayerSeasonStatistics");
            
            // Configure primary key
            builder.HasKey(s => s.Id);

            // Configure foreign keys
            builder.Property(s => s.PlayerId)
                .IsRequired()
                .HasComment("ID of the player these statistics belong to");

            builder.Property(s => s.TeamId)
                .IsRequired()
                .HasComment("ID of the team the player played for");

            builder.Property(s => s.CompetitionId)
                .IsRequired()
                .HasComment("ID of the competition these statistics are for");

            // Configure basic statistics
            builder.Property(s => s.GamesPlayed)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of games played");

            builder.Property(s => s.Goals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Goals scored");

            builder.Property(s => s.Assists)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Assists made");

            builder.Property(s => s.Points)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total points (goals + assists)");

            builder.Property(s => s.PenaltyMinutes)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Penalty minutes");

            builder.Property(s => s.PlusMinusRating)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Plus/minus rating");

            // Configure shot statistics
            builder.Property(s => s.ShotsOnGoal)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Shots on goal");

            builder.Property(s => s.ShotPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Shooting percentage");

            // Configure power play statistics
            builder.Property(s => s.PowerPlayGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play goals");

            builder.Property(s => s.PowerPlayAssists)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play assists");

            // Configure short-handed statistics
            builder.Property(s => s.ShortHandedGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Short-handed goals");

            builder.Property(s => s.ShortHandedAssists)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Short-handed assists");

            // Configure special goals
            builder.Property(s => s.GameWinningGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Game-winning goals");

            builder.Property(s => s.OvertimeGoals)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Overtime goals");

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

            // Configure indexes for performance
            builder.HasIndex(s => s.PlayerId)
                .HasDatabaseName("IX_FloorballPlayerSeasonStatistics_PlayerId");

            builder.HasIndex(s => s.TeamId)
                .HasDatabaseName("IX_FloorballPlayerSeasonStatistics_TeamId");

            builder.HasIndex(s => s.CompetitionId)
                .HasDatabaseName("IX_FloorballPlayerSeasonStatistics_SeasonId");

            builder.HasIndex(s => new { s.PlayerId, s.TeamId, s.CompetitionId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballPlayerSeasonStatistics_PlayerId_TeamId_SeasonId");

            // Index for leaderboard queries
            builder.HasIndex(s => new { s.CompetitionId, s.Goals })
                .HasDatabaseName("IX_FloorballPlayerSeasonStatistics_SeasonId_Goals");

            builder.HasIndex(s => new { s.CompetitionId, s.Assists })
                .HasDatabaseName("IX_FloorballPlayerSeasonStatistics_SeasonId_Assists");

            builder.HasIndex(s => new { s.CompetitionId, s.Points })
                .HasDatabaseName("IX_FloorballPlayerSeasonStatistics_SeasonId_Points");

            // Configure base entity properties
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasComment("UTC timestamp when the entity was created");

            builder.Property(s => s.UpdatedAt)
                .HasComment("UTC timestamp when the entity was last updated");
        }
    }
}
