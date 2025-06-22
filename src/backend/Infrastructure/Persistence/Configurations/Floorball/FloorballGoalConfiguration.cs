using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Floorball;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for FloorballGoal entity
/// </summary>
public class FloorballGoalConfiguration : IEntityTypeConfiguration<FloorballGoal>
{
    /// <summary>
    /// Configures the FloorballGoal entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<FloorballGoal> builder)
    {
        // Properties specific to FloorballGoal
        builder.Property(g => g.ScoringPlayerId)
            .IsRequired(false);

        builder.Property(g => g.AssistingPlayerId)
            .IsRequired(false);

        builder.Property(g => g.GoalType)
            .IsRequired(false)
            .HasConversion<int?>();

        // Foreign key relationships
        builder.HasOne<FloorballPlayer>()
            .WithMany()
            .HasForeignKey(g => g.ScoringPlayerId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne<FloorballPlayer>()
            .WithMany()
            .HasForeignKey(g => g.AssistingPlayerId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes specific to goals
        builder.HasIndex(g => g.ScoringPlayerId)
            .HasDatabaseName("IX_FloorballGoal_ScoringPlayerId")
            .HasFilter("\"ScoringPlayerId\" IS NOT NULL");

        builder.HasIndex(g => g.AssistingPlayerId)
            .HasDatabaseName("IX_FloorballGoal_AssistingPlayerId")
            .HasFilter("\"AssistingPlayerId\" IS NOT NULL");

        builder.HasIndex(g => g.GoalType)
            .HasDatabaseName("IX_FloorballGoal_GoalType")
            .HasFilter("\"GoalType\" IS NOT NULL");
    }
} 