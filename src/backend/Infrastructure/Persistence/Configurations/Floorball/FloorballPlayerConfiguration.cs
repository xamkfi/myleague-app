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
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PersonId)
                .IsRequired();

            builder.HasOne(p => p.Person)
                .WithMany()
                .HasForeignKey(p => p.PersonId)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .IsRequired();

            builder.OwnsOne(p => p.Position, positionBuilder =>
            {
                positionBuilder.Property(p => p.PrimaryPosition)
                    .IsRequired()
                    .HasConversion<string>();

                positionBuilder.Property(p => p.SecondaryPosition)
                    .HasConversion<string>();

                positionBuilder.Property(p => p.CanPlayAsGoalkeeper)
                    .IsRequired();
            });

            builder.Property(p => p.CareerGoals)
                .IsRequired();

            builder.Property(p => p.CareerAssists)
                .IsRequired();
        }
    }
} 