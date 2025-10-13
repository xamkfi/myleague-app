using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Common
{
    /// <summary>
    /// Entity Framework configuration for the Division entity.
    /// </summary>
    public class DivisionConfiguration : IEntityTypeConfiguration<Division>
    {
        /// <summary>
        /// Configures the entity mapping for Division.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<Division> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.Level)
                .IsRequired();

            builder.Property(d => d.SportType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(d => d.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            // Create unique constraint on Name + SportType combination
            builder.HasIndex(d => new { d.Name, d.SportType })
                .IsUnique()
                .HasDatabaseName("IX_Divisions_Name_SportType");

            // Create index on SportType for efficient filtering
            builder.HasIndex(d => d.SportType)
                .HasDatabaseName("IX_Divisions_SportType");

            // Create index on IsActive for efficient filtering
            builder.HasIndex(d => d.IsActive)
                .HasDatabaseName("IX_Divisions_IsActive");

            // Create composite index on SportType + IsActive for common queries
            builder.HasIndex(d => new { d.SportType, d.IsActive })
                .HasDatabaseName("IX_Divisions_SportType_IsActive");

            // Create index on Level for ordering
            builder.HasIndex(d => d.Level)
                .HasDatabaseName("IX_Divisions_Level");
        }
    }
} 