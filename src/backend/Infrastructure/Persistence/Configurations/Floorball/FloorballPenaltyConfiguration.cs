using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Floorball;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for FloorballPenalty entity
/// </summary>
public class FloorballPenaltyConfiguration : IEntityTypeConfiguration<FloorballPenalty>
{
    /// <summary>
    /// Configures the FloorballPenalty entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<FloorballPenalty> builder)
    {
        // Properties specific to FloorballPenalty
        builder.Property(p => p.PlayerId)
            .IsRequired(false);

        builder.Property(p => p.PenaltyType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.DurationInMinutes)
            .IsRequired();

        // Foreign key relationship
        builder.HasOne<FloorballPlayer>()
            .WithMany()
            .HasForeignKey(p => p.PlayerId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Constraints specific to penalties
        builder.HasCheckConstraint("CK_FloorballPenalty_DurationInMinutes", "\"DurationInMinutes\" > 0");

        // Indexes specific to penalties
        builder.HasIndex(p => p.PlayerId)
            .HasDatabaseName("IX_FloorballPenalty_PlayerId")
            .HasFilter("\"PlayerId\" IS NOT NULL");

        builder.HasIndex(p => p.PenaltyType)
            .HasDatabaseName("IX_FloorballPenalty_PenaltyType");

        builder.HasIndex(p => p.DurationInMinutes)
            .HasDatabaseName("IX_FloorballPenalty_DurationInMinutes");
    }
} 