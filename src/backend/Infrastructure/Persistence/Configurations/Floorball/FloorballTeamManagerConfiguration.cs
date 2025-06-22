using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Floorball;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for FloorballTeamManager entity
/// </summary>
public class FloorballTeamManagerConfiguration : IEntityTypeConfiguration<FloorballTeamManager>
{
    /// <summary>
    /// Configures the FloorballTeamManager entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<FloorballTeamManager> builder)
    {
        // Table name
        builder.ToTable("FloorballTeamManagers");

        // Primary key
        builder.HasKey(tm => tm.Id);

        // Properties
        builder.Property(tm => tm.Id)
            .IsRequired();

        builder.Property(tm => tm.PersonId)
            .IsRequired();

        builder.Property(tm => tm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(tm => tm.PrimaryResponsibility)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(tm => tm.YearsOfExperience)
            .IsRequired()
            .HasDefaultValue(0);

        // Constraints
        builder.HasCheckConstraint("CK_FloorballTeamManager_YearsOfExperience", "\"YearsOfExperience\" >= 0");

        // Indexes
        builder.HasIndex(tm => tm.PersonId)
            .IsUnique()
            .HasDatabaseName("IX_FloorballTeamManager_PersonId");

        builder.HasIndex(tm => tm.IsActive)
            .HasDatabaseName("IX_FloorballTeamManager_IsActive");
    }
} 