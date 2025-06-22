using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Floorball;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball;

/// <summary>
/// Entity Framework configuration for FloorballCoach entity
/// </summary>
public class FloorballCoachConfiguration : IEntityTypeConfiguration<FloorballCoach>
{
    /// <summary>
    /// Configures the FloorballCoach entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<FloorballCoach> builder)
    {
        // Table name
        builder.ToTable("FloorballCoaches");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .IsRequired();

        builder.Property(c => c.PersonId)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.YearsOfExperience)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.CertificationLevel)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.Specialization)
            .HasMaxLength(100)
            .IsRequired(false);

        // Constraints
        builder.HasCheckConstraint("CK_FloorballCoach_YearsOfExperience", "\"YearsOfExperience\" >= 0");

        // Indexes
        builder.HasIndex(c => c.PersonId)
            .IsUnique()
            .HasDatabaseName("IX_FloorballCoach_PersonId");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_FloorballCoach_IsActive");

        builder.HasIndex(c => c.Specialization)
            .HasDatabaseName("IX_FloorballCoach_Specialization")
            .HasFilter("\"Specialization\" IS NOT NULL");
    }
} 