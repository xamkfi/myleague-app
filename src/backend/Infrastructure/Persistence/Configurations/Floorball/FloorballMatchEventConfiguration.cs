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
            .HasValue<FloorballPenalty>("Penalty");

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

        // Configure FloorballPenalty-specific properties
        builder.Property<Guid?>("PlayerId")
            .IsRequired(false);

        builder.Property<int?>("PenaltyType")
            .IsRequired(false);

        builder.Property<int?>("DurationInMinutes")
            .IsRequired(false);

        // Configure FloorballGoal-specific properties
        builder.Property<Guid?>("ScoringPlayerId")
            .IsRequired(false);

        builder.Property<Guid?>("AssistingPlayerId")
            .IsRequired(false);

        builder.Property<int?>("GoalType")
            .IsRequired(false);

        builder.Property<bool?>("IsOvertime")
            .IsRequired(false);

        builder.Property<bool?>("IsShootout")
            .IsRequired(false);

        // Foreign key relationships
        builder.HasOne<FloorballMatch>()
            .WithMany(m => m.Events)
            .HasForeignKey(e => e.MatchId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne<FloorballTeam>()
            .WithMany()
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Foreign key for penalty player
        builder.HasOne<FloorballPlayer>()
            .WithMany()
            .HasForeignKey("PlayerId")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Foreign key for goal scorer
        builder.HasOne<FloorballPlayer>()
            .WithMany()
            .HasForeignKey("ScoringPlayerId")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Foreign key for goal assister
        builder.HasOne<FloorballPlayer>()
            .WithMany()
            .HasForeignKey("AssistingPlayerId")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(e => e.MatchId)
            .HasDatabaseName("IX_FloorballMatchEvent_MatchId");

        builder.HasIndex(e => e.TeamId)
            .HasDatabaseName("IX_FloorballMatchEvent_TeamId");

        builder.HasIndex(e => new { e.MatchId, e.PeriodNumber, e.TimeInSeconds })
            .HasDatabaseName("IX_FloorballMatchEvent_MatchId_Period_Time");

        // Penalty-specific indexes
        builder.HasIndex("PlayerId")
            .HasDatabaseName("IX_FloorballMatchEvent_PlayerId")
            .HasFilter("\"PlayerId\" IS NOT NULL");

        builder.HasIndex("PenaltyType")
            .HasDatabaseName("IX_FloorballMatchEvent_PenaltyType")
            .HasFilter("\"PenaltyType\" IS NOT NULL");

        builder.HasIndex("DurationInMinutes")
            .HasDatabaseName("IX_FloorballMatchEvent_DurationInMinutes")
            .HasFilter("\"DurationInMinutes\" IS NOT NULL");

        // Goal-specific indexes
        builder.HasIndex("ScoringPlayerId")
            .HasDatabaseName("IX_FloorballMatchEvent_ScoringPlayerId")
            .HasFilter("\"ScoringPlayerId\" IS NOT NULL");

        builder.HasIndex("AssistingPlayerId")
            .HasDatabaseName("IX_FloorballMatchEvent_AssistingPlayerId")
            .HasFilter("\"AssistingPlayerId\" IS NOT NULL");

        builder.HasIndex("GoalType")
            .HasDatabaseName("IX_FloorballMatchEvent_GoalType")
            .HasFilter("\"GoalType\" IS NOT NULL");
    }
} 