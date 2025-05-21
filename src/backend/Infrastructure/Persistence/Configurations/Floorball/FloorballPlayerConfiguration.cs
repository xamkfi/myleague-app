using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballPlayer entity.
    /// </summary>
    public class FloorballPlayerConfiguration : IEntityTypeConfiguration<FloorballPlayer>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballPlayer.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballPlayer> builder)
        {
            // FloorballPlayer inherits from Person, so primary key is configured there
            
            builder.Property(p => p.IsActive)
                .IsRequired();

            builder.Property(p => p.PreferredPosition)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(p => p.CareerGoals)
                .IsRequired();

            builder.Property(p => p.CareerAssists)
                .IsRequired();
        }
    }
} 