using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for the FloorballGoal entity.
/// </summary>
public class FloorballGoalConfiguration : IEntityTypeConfiguration<FloorballGoal>
{
    /// <summary>
    /// Configures the entity mapping for FloorballGoal.
    /// </summary>
    /// <param name="b"></param>
    public void Configure(EntityTypeBuilder<FloorballGoal> b)
    {
        b.Property(g => g.ScoringPlayerId).IsRequired(false);
        b.Property(g => g.AssistingPlayerId).IsRequired(false);
        b.Property(g => g.GoalType).IsRequired(false);


        b.HasIndex(g => g.ScoringPlayerId)
         .HasDatabaseName("IX_FloorballMatchEvent_ScoringPlayerId");

        b.HasIndex(g => g.AssistingPlayerId)
         .HasDatabaseName("IX_FloorballMatchEvent_AssistingPlayerId");

        b.HasIndex(g => g.GoalType)
         .HasDatabaseName("IX_FloorballMatchEvent_GoalType");
    }

}
