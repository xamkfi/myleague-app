using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Floorball;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for FloorballMatchEvent entity and its derived types
/// </summary>
public class FloorballMatchEventConfiguration : IEntityTypeConfiguration<FloorballMatchEvent>
{
    /// <summary>
    /// Configures the FloorballMatchEvent entity and its inheritance hierarchy
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<FloorballMatchEvent> builder)
    {
        // Table name and constraints
        builder.ToTable("FloorballMatchEvents", t => 
        {
            t.HasCheckConstraint("CK_FloorballMatchEvent_PeriodNumber", "\"PeriodNumber\" > 0");
            t.HasCheckConstraint("CK_FloorballMatchEvent_TimeInSeconds", "\"TimeInSeconds\" >= 0");
            t.HasCheckConstraint("CK_FloorballPenalty_DurationInMinutes", "\"DurationInMinutes\" IS NULL OR \"DurationInMinutes\" > 0");
        });

        // Primary key
        builder.HasKey(e => e.Id);

        // Configure inheritance - Table Per Hierarchy (TPH)
        builder.HasDiscriminator<string>("EventType")
            .HasValue<FloorballGoal>("Goal")
            .HasValue<FloorballPenalty>("Penalty")
            .HasValue<FloorballSave>("Save");
        
        // Properties
        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.MatchId)
            .IsRequired();

        builder.Property(e => e.TeamId)
            .IsRequired();

        builder.Property(e => e.PeriodNumber)
            .IsRequired();

        builder.Property(e => e.TimeInSeconds)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);

        // Computed column for formatted time
        builder.Ignore(e => e.FormattedTime);

        // Foreign key relationships
        builder.HasOne<FloorballTeam>()
            .WithMany()
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.MatchId)
            .HasDatabaseName("IX_FloorballMatchEvent_MatchId");

        builder.HasIndex(e => e.TeamId)
            .HasDatabaseName("IX_FloorballMatchEvent_TeamId");

        builder.HasIndex(e => new { e.MatchId, e.PeriodNumber, e.TimeInSeconds })
            .HasDatabaseName("IX_FloorballMatchEvent_MatchId_Period_Time");
    }
}
