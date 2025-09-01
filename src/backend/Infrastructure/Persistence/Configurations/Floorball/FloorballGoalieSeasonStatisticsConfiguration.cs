using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballGoalieSeasonStatistics entity.
    /// </summary>
    public class FloorballGoalieSeasonStatisticsConfiguration : IEntityTypeConfiguration<FloorballGoalieSeasonStatistics>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballGoalieSeasonStatistics.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballGoalieSeasonStatistics> builder)
        {
            // Configure table
            builder.ToTable("FloorballGoalieSeasonStatistics");
            
            // Configure primary key
            builder.HasKey(s => s.Id);

            // Configure foreign keys
            builder.Property(s => s.PlayerId)
                .IsRequired()
                .HasComment("ID of the goalie these statistics belong to");

            builder.Property(s => s.TeamId)
                .IsRequired()
                .HasComment("ID of the team the goalie played for");

            builder.Property(s => s.SeasonId)
                .IsRequired()
                .HasComment("ID of the season these statistics are for");

            // Ignore navigation properties to prevent cross-context issues
            builder.Ignore(s => s.Player);
            builder.Ignore(s => s.Team);
            builder.Ignore(s => s.Season);

            // Configure basic goalie statistics
            builder.Property(s => s.GamesPlayed)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of games played");

            builder.Property(s => s.GamesStarted)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of games started");

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
                .HasComment("Number of ties");

            // Configure save statistics
            builder.Property(s => s.Saves)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total saves made");

            builder.Property(s => s.ShotsAgainst)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total shots faced");

            builder.Property(s => s.SavePercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Save percentage");

            builder.Property(s => s.GoalsAgainst)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Goals allowed");

            builder.Property(s => s.GoalsAgainstAverage)
                .HasColumnType("decimal(4,2)")
                .HasDefaultValue(0)
                .HasComment("Goals against average");

            builder.Property(s => s.Shutouts)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of shutouts");

            builder.Property(s => s.MinutesPlayed)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total minutes played");

            // Configure power play statistics
            builder.Property(s => s.PowerPlaySaves)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play saves");

            builder.Property(s => s.PowerPlayShotsAgainst)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Power play shots faced");

            builder.Property(s => s.PowerPlaySavePercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Power play save percentage");

            // Configure short-handed statistics
            builder.Property(s => s.ShortHandedSaves)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Short-handed saves");

            builder.Property(s => s.ShortHandedShotsAgainst)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Short-handed shots faced");

            builder.Property(s => s.ShortHandedSavePercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .HasComment("Short-handed save percentage");

            // Configure indexes for performance
            builder.HasIndex(s => s.PlayerId)
                .HasDatabaseName("IX_FloorballGoalieSeasonStatistics_PlayerId");

            builder.HasIndex(s => s.TeamId)
                .HasDatabaseName("IX_FloorballGoalieSeasonStatistics_TeamId");

            builder.HasIndex(s => s.SeasonId)
                .HasDatabaseName("IX_FloorballGoalieSeasonStatistics_SeasonId");

            builder.HasIndex(s => new { s.PlayerId, s.TeamId, s.SeasonId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballGoalieSeasonStatistics_PlayerId_TeamId_SeasonId");

            // Index for leaderboard queries
            builder.HasIndex(s => new { s.SeasonId, s.SavePercentage })
                .HasDatabaseName("IX_FloorballGoalieSeasonStatistics_SeasonId_SavePercentage");

            builder.HasIndex(s => new { s.SeasonId, s.GoalsAgainstAverage })
                .HasDatabaseName("IX_FloorballGoalieSeasonStatistics_SeasonId_GAA");

            builder.HasIndex(s => new { s.SeasonId, s.Wins })
                .HasDatabaseName("IX_FloorballGoalieSeasonStatistics_SeasonId_Wins");

            // Configure base entity properties
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasComment("UTC timestamp when the entity was created");

            builder.Property(s => s.UpdatedAt)
                .HasComment("UTC timestamp when the entity was last updated");
        }
    }
}
