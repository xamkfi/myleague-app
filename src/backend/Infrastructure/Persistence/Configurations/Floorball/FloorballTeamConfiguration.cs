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

            // Ignore the Division navigation property since it's in a different DbContext (CommonDbContext)
            // We use DivisionId as foreign key instead for cross-context relationships
            builder.Ignore(t => t.Division);

            builder.Property(t => t.DivisionId)
                .IsRequired();

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

            // Ignore Club navigation property to prevent cross-context entity discovery
            builder.Ignore(t => t.Club);


            // We maintain the ClubId as a foreign key for reference
            // builder.Property("ClubId")

            // Configure the ClubId as a required property for cross-context reference
            builder.Property(t => t.ClubId)
                .IsRequired();

            // Configure the relationship with FloorballTeamPlayer
            builder.HasMany(t => t.Roster)
                .WithOne()
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }
    }
} 
