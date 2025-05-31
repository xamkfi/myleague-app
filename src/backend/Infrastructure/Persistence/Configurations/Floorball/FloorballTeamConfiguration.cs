using Domain.Entities.Floorball;
using Domain.ValueObjects.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTeam entity.
    /// </summary>
    public class FloorballTeamConfiguration : IEntityTypeConfiguration<FloorballTeam>
    {
        /// <summary>
        /// Configures the entity mapping for FloorballTeam.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        public void Configure(EntityTypeBuilder<FloorballTeam> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Division)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.HomeArena)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.PrimaryJerseyColor)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.SecondaryJerseyColor)
                .HasMaxLength(50);

            builder.Property(x => x.TeamCategory)
                .IsRequired()
                .HasConversion<string>();

            // We maintain the ClubId as a foreign key for reference
            builder.Property("ClubId")
                .IsRequired();

            // Add index on ClubId for better query performance
            builder.HasIndex("ClubId");

            // Ignore Club navigation property to prevent cross-context entity discovery
            builder.Ignore(t => t.Club);

            // Configure the owned FloorballTeamPlayer collection
            builder.OwnsMany(t => t.Roster, rosterBuilder =>
            {
                rosterBuilder.WithOwner().HasForeignKey("TeamId");
                rosterBuilder.Property<Guid>("Id").ValueGeneratedOnAdd();
                rosterBuilder.HasKey("Id");
                
                rosterBuilder.Property(p => p.TeamId)
                    .IsRequired();
                
                rosterBuilder.Property(p => p.PlayerId)
                    .IsRequired();
                
                rosterBuilder.Property(p => p.Position)
                    .IsRequired()
                    .HasConversion<string>();
                
                rosterBuilder.Property(p => p.JerseyNumber);
                
                rosterBuilder.Property(p => p.IsActive)
                    .IsRequired();
                
                rosterBuilder.Property(p => p.GamesPlayed)
                    .IsRequired();
                
                rosterBuilder.Property(p => p.Goals)
                    .IsRequired();
                
                rosterBuilder.Property(p => p.Assists)
                    .IsRequired();
                
                rosterBuilder.Property(p => p.PenaltyMinutes)
                    .IsRequired();

                // Add foreign key relationship to FloorballPlayer
                rosterBuilder.HasOne<FloorballPlayer>()
                    .WithMany()
                    .HasForeignKey(p => p.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add index on PlayerId for better query performance
                rosterBuilder.HasIndex(p => p.PlayerId);
            });
        }
    }
} 
