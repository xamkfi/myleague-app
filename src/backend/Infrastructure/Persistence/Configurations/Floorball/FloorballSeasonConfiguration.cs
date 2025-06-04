using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballSeason entity.
    /// </summary>
    public class FloorballSeasonConfiguration : IEntityTypeConfiguration<FloorballSeason>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballSeason.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballSeason> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Division)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.EndDate)
                .IsRequired();

            builder.Property(s => s.IsActive)
                .IsRequired();

            builder.Property(s => s.IsCompleted)
                .IsRequired();

            // Ignore complex relationships for now to avoid navigationName issues
            // These can be managed at the application level
            builder.Ignore(s => s.Teams);
            builder.Ignore(s => s.Matches);
        }
    }
} 