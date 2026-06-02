using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLeague.Infrastructure.Persistence.Configurations;

namespace MyLeague.Infrastructure.Persistence.Configurations.Floorball
{
    /// <summary>
    /// Entity Framework configuration for the FloorballTeamPlayer entity.
    /// </summary>
    public class FloorballTeamPlayerConfiguration : BaseEntityConfiguration<FloorballTeamPlayer>
    {
        /// <summary>
        /// Configures the entity-specific properties for FloorballTeamPlayer.
        /// </summary>
        /// <param name="builder">The entity type builder.</param>
        protected override void ConfigureEntity(EntityTypeBuilder<FloorballTeamPlayer> builder)
        {
            // Configure table name
            builder.ToTable("FloorballTeamPlayers");

            // Configure properties
            builder.Property(p => p.TeamId)
                .IsRequired();
            
            builder.Property(p => p.PlayerId)
                .IsRequired();
            
            builder.Property(p => p.Position)
                .IsRequired()
                .HasConversion<string>();
            
            builder.Property(p => p.JerseyNumber);

            builder.Property(p => p.RequestedJerseyNumber);

            // HasJerseyNumberSubstituted is a computed property on the domain entity (derived
            // from JerseyNumber vs RequestedJerseyNumber), so EF must not map it as a column.
            builder.Ignore(p => p.HasJerseyNumberSubstituted);

            builder.Property(p => p.IsActive)
                .IsRequired();
            
            builder.Property(p => p.GamesPlayed)
                .IsRequired();
            
            builder.Property(p => p.Goals)
                .IsRequired();
            
            builder.Property(p => p.Assists)
                .IsRequired();
            
            builder.Property(p => p.PenaltyMinutes)
                .IsRequired();

            // Configure relationships
            builder.HasOne<FloorballTeam>()
                .WithMany(t => t.Roster)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne<FloorballPlayer>()
                .WithMany()
                .HasForeignKey(p => p.PlayerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configure indexes
            builder.HasIndex(p => p.PlayerId)
                .HasDatabaseName("IX_FloorballTeamPlayer_PlayerId");

            builder.HasIndex(p => new { p.TeamId, p.PlayerId })
                .IsUnique()
                .HasDatabaseName("IX_FloorballTeamPlayer_TeamId_PlayerId");

            builder.HasIndex(p => new { p.TeamId, p.JerseyNumber })
                .IsUnique()
                .HasFilter("\"JerseyNumber\" IS NOT NULL")
                .HasDatabaseName("IX_FloorballTeamPlayer_TeamId_JerseyNumber");
        }
    }
} 